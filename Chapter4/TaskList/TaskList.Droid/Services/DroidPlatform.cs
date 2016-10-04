using Android.App;
using Android.Content;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Plugin.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskList.Abstractions;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(TaskList.Droid.Services.DroidPlatform))]
namespace TaskList.Droid.Services
{
    public class DroidPlatform : IPlatform
    {
        /// <summary>
        /// Identifier to tag the data we store in the secure store
        /// </summary>
        private const string ServiceIdentifier = "thebook";

        /// <summary>
        /// Reference to the secure store for the device
        /// </summary>
        public AccountStore AccountStore { get; private set; }

        /// <summary>
        /// Reference to the root view of the application
        /// </summary>
        public Context RootView { get; private set; }

        /// <summary>
        /// Initialize the platform provider - called from the MainActivity.cs file
        /// </summary>
        /// <param name="context">The root view</param>
        public void Init(Context context)
        {
            RootView = context;
            AccountStore = AccountStore.Create(context);
        }

        /// <summary>
        /// Login using AAD with client-flow
        /// </summary>
        /// <returns>(async) the token</returns>
        private async Task<string> LoginADALAsync()
        {
            var authContext = new AuthenticationContext(Locations.AadAuthority);
            if (authContext.TokenCache.ReadItems().Any())
            {
                authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
            }
            var authResult = await authContext.AcquireTokenAsync(
                Locations.AppServiceUrl,
                Locations.AadClientId,
                new Uri(Locations.AadRedirectUri),
                new PlatformParameters((Activity)RootView));
            return authResult.AccessToken;
        }

        #region IPlatform Interface
        /// <summary>
        /// Log the user in, returning the mobile services user
        /// </summary>
        /// <param name="client">The mobile service client</param>
        /// <returns>The mobile service user record</returns>
        public async Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            var accessToken = await LoginADALAsync();
            var zumoPayload = new JObject();
            zumoPayload["access_token"] = accessToken;
            return await client.LoginAsync("aad", zumoPayload);
        }

        /// <summary>
        /// Log the user out.
        /// </summary>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LogoutAsync()
        {
            // Deliberate: Do nothing
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <summary>
        /// Remove the user records from the secure store
        /// </summary>
        public void RemoveTokenFromSecureStore()
        {
            var accounts = AccountStore.FindAccountsForService(ServiceIdentifier);
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    AccountStore.Delete(acct, ServiceIdentifier);
                }
            }
        }

        /// <summary>
        /// Retrieve the user authentication token from the secure store
        /// </summary>
        /// <returns>The user record</returns>
        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            var accounts = AccountStore.FindAccountsForService(ServiceIdentifier);
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    string token;
                    if (acct.Properties.TryGetValue("token", out token))
                    {
                        return new MobileServiceUser(acct.Username) { MobileServiceAuthenticationToken = token };
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Store the MobileServiceUser authentication token into the secure store
        /// </summary>
        /// <param name="user">The user record</param>
        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            var account = new Account(user.UserId);
            account.Properties.Add("token", user.MobileServiceAuthenticationToken);
            AccountStore.Save(account, ServiceIdentifier);
        }

        /// <summary>
        /// Picks a photo for uploading
        /// </summary>
        /// <returns>A Stream for the photo</returns>
        public async Task<Stream> GetUploadFileAsync()
        {
            var mediaPlugin = CrossMedia.Current;
            var mainPage = Xamarin.Forms.Application.Current.MainPage;

            await mediaPlugin.Initialize();

            if (mediaPlugin.IsPickPhotoSupported)
            {
                var mediaFile = await mediaPlugin.PickPhotoAsync();
                return mediaFile.GetStream();
            }
            else
            {
                await mainPage.DisplayAlert("Media Service Unavailable", "Cannot pick photo", "OK");
                return null;
            }
        }

        /// <summary>
        /// Obtains the platform-specific path for the sync-store database.
        /// </summary>
        /// <returns>Path to the syncstore on the local device</returns>
        public string GetSyncStorePath()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var dbName = $"{ServiceIdentifier}.db";

            var dbPath = Path.Combine(basePath, dbName);
            if (!File.Exists(dbPath))
            {
                File.Create(dbPath).Dispose();
            }
            return dbPath;
        }
        #endregion
    }
}
