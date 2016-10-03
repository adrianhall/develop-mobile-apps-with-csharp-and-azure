using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.Services
{
    public class AzureCloudService : ICloudService
    {
        /// <summary>
        /// The Client reference to the Azure Mobile App
        /// </summary>
        private MobileServiceClient Client { get; set; }

        /// <summary>
        /// The cache for the App Service Identity
        /// </summary>
        private List<AppServiceIdentity> Identities { get; set; }

        /// <summary>
        /// Reference to the platform-specific code
        /// </summary>
        private IPlatform PlatformProvider { get; set; }

        /// <summary>
        /// Constructor: Create a new Cloud Service connection.
        /// </summary>
        public AzureCloudService()
        {
            Client = new MobileServiceClient(
                Locations.AppServiceUrl,
                new AuthenticationDelegatingHandler());

            if (Locations.AlternateLoginHost != null)
            {
                Client.AlternateLoginHost = new Uri(Locations.AlternateLoginHost);
            }

            PlatformProvider = DependencyService.Get<IPlatform>();
            if (PlatformProvider == null)
            {
                throw new InvalidOperationException("No Platform Provider");
            }
        }

        #region Offline Sync
        private async Task InitializeAsync()
        {
            // Short circuit - local database is already initialized
            if (Client.SyncContext.IsInitialized)
            {
                Debug.WriteLine("InitializeAsync: Short Circuit");
                return;
            }

            // Create a reference to the local sqlite store
            Debug.WriteLine("InitializeAsync: Initializing store");
            var store = new MobileServiceSQLiteStore(PlatformProvider.GetSyncStore());

            // Define the database schema
            Debug.WriteLine("InitializeAsync: Defining Datastore");
            store.DefineTable<Tag>();
            store.DefineTable<TodoItem>();

            // Actually create the store and update the schema
            Debug.WriteLine("InitializeAsync: Initializing SyncContext");
            await Client.SyncContext.InitializeAsync(store);

            // Do the sync
            Debug.WriteLine("InitializeAsync: Syncing Offline Cache");
            await SyncOfflineCacheAsync();
        }

        public async Task SyncOfflineCacheAsync()
        {
            Debug.WriteLine("SyncOfflineCacheAsync: Initializing...");
            await InitializeAsync();

            // Push the Operations Queue to the mobile backend
            Debug.WriteLine("SyncOfflineCacheAsync: Pushing Changes");
            await Client.SyncContext.PushAsync();

            // Pull each sync table
            Debug.WriteLine("SyncOfflineCacheAsync: Pulling tags table");
            var tagTable = await GetTableAsync<Tag>(); await tagTable.PullAsync();

            Debug.WriteLine("SyncOfflineCacheAsync: Pulling tasks table");
            var taskTable = await GetTableAsync<TodoItem>(); await taskTable.PullAsync();
        }
        #endregion

        /// <summary>
        /// Determine if the JWT token provided is expired or not.
        /// </summary>
        /// <param name="token">The token to check</param>
        /// <returns>true if the token is expired</returns>
        private bool IsTokenExpired(string token)
        {
            // Get just the JWT part of the token (without the signature).
            var jwt = token.Split(new Char[] { '.' })[1];

            // Undo the URL encoding.
            jwt = jwt.Replace('-', '+').Replace('_', '/');
            switch (jwt.Length % 4)
            {
                case 0: break;
                case 2: jwt += "=="; break;
                case 3: jwt += "="; break;
                default:
                    throw new ArgumentException("The token is not a valid Base64 string.");
            }

            // Convert to a JSON String
            var bytes = Convert.FromBase64String(jwt);
            string jsonString = UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            // Parse as JSON object and get the exp field value,
            // which is the expiration date as a JavaScript primative date.
            JObject jsonObj = JObject.Parse(jsonString);
            var exp = Convert.ToDouble(jsonObj["exp"].ToString());

            // Calculate the expiration by adding the exp value (in seconds) to the
            // base date of 1/1/1970.
            DateTime minTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expire = minTime.AddSeconds(exp);
            return expire < DateTime.UtcNow;
        }

        #region ICloudService Interface
        /// <summary>
        /// Return the first identity set of claims
        /// </summary>
        /// <returns>The AppServiceIdentity (or null)</returns>
        public async Task<AppServiceIdentity> GetIdentityAsync()
        {
            if (Client.CurrentUser == null || Client.CurrentUser?.MobileServiceAuthenticationToken == null)
            {
                throw new InvalidOperationException("Not Authenticated");
            }
            if (Identities == null)
            {
                Identities = await Client.InvokeApiAsync<List<AppServiceIdentity>>("/.auth/me");
            }
            if (Identities.Count > 0)
            {
                return Identities[0];
            }
            return null;
        }

        /// <summary>
        /// Returns a link to the specific table.
        /// </summary>
        /// <typeparam name="T">The model</typeparam>
        /// <returns>The table reference</returns>
        public async Task<ICloudTable<T>> GetTableAsync<T>() where T : TableData
        {
            await InitializeAsync();
            return new AzureCloudTable<T>(Client);
        }

        /// <summary>
        /// Try to log in to the backend
        /// </summary>
        /// <returns>The mobile service user</returns>
        public async Task<MobileServiceUser> LoginAsync()
        {
            Client.CurrentUser = PlatformProvider.RetrieveTokenFromSecureStore();
            if (Client.CurrentUser != null)
            {
                Debug.WriteLine($"LoginAsync: user = {Client.CurrentUser.UserId}");
                try
                {
                    var refreshedUser = await Client.RefreshUserAsync();
                    if (refreshedUser != null)
                    {
                        Debug.WriteLine($"LoginAsync: User Refreshed!  Token = {refreshedUser.MobileServiceAuthenticationToken}");
                        PlatformProvider.StoreTokenInSecureStore(refreshedUser);
                        return Client.CurrentUser;
                    }
                }
                catch (Exception refreshException)
                {
                    Debug.WriteLine($"Could not refresh token: {refreshException.Message}");
                }
            }

            if (Client.CurrentUser != null && !IsTokenExpired(Client.CurrentUser.MobileServiceAuthenticationToken))
            {
                Debug.WriteLine($"LoginAsync: From Store: Token = {Client.CurrentUser.MobileServiceAuthenticationToken}");
                return Client.CurrentUser;
            }

            Debug.WriteLine($"LoginAsync: Need to authenticate user");
            await PlatformProvider.LoginAsync(Client);
            if (Client.CurrentUser != null)
            {
                PlatformProvider.StoreTokenInSecureStore(Client.CurrentUser);
                Debug.WriteLine($"LoginAsync: From Login: Token = {Client.CurrentUser.MobileServiceAuthenticationToken}");
                return Client.CurrentUser;
            }

            PlatformProvider.RemoveTokenFromSecureStore();
            return null;
        }

        /// <summary>
        /// Swap the original token for a new token
        /// </summary>
        /// <param name="user">The user object</param>
        /// <returns>The new user object</returns>
        public async Task<MobileServiceUser> UpdateUserAsync(MobileServiceUser user)
        {
            Debug.WriteLine($"Updating user {user.UserId} # {user.MobileServiceAuthenticationToken}");
            try
            {
                var loginResult = await Client.InvokeApiAsync<LoginResult>("/auth/login/custom", HttpMethod.Get, null);
                Client.CurrentUser.MobileServiceAuthenticationToken = loginResult.AuthenticationToken;
                Client.CurrentUser.UserId = loginResult.UserId;
            }
            catch (Exception ex)
            {
                PlatformProvider.RemoveTokenFromSecureStore();
                Debug.WriteLine($"Updating User Failed: {ex.Message}");
                throw;
            }
            return Client.CurrentUser;
        }

        /// <summary>
        /// Log out of the system.
        /// </summary>
        /// <returns></returns>
        public async Task LogoutAsync()
        {
            if (Client.CurrentUser == null || Client.CurrentUser?.MobileServiceAuthenticationToken == null)
            {
                return;
            }

            // Log out of the client-flow identity provider
            await PlatformProvider.LogoutAsync();

            // Remove the token from the token store
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", Client.CurrentUser.MobileServiceAuthenticationToken);
                await httpClient.GetAsync(new Uri($"{Client.MobileAppUri}/.auth/logout"));
            }

            // Remove the token from the application secure store
            PlatformProvider.RemoveTokenFromSecureStore();

            // Log out of the client
            await Client.LogoutAsync();
        }
        #endregion
    }

    public class LoginResult
    {
        [JsonProperty(PropertyName = "authenticationToken")]
        public string AuthenticationToken { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }
    }
}
