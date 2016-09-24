using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
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
        /// Reference to the client connection for Azure Mobile Apps
        /// </summary>
        public MobileServiceClient Client { get; set; }

        /// <summary>
        /// List of identities currently logged in
        /// </summary>
        public List<AppServiceIdentity> Identities { get; set; }

        /// <summary>
        /// The platform-specific code
        /// </summary>
        public IPlatform PlatformProvider { get; set; }

        /// <summary>
        /// Reference to the sync table for Tasks
        /// </summary>
        public IMobileServiceSyncTable<TodoItem> TaskTable { get; set; }

        public AzureCloudService()
        {
            Client = new MobileServiceClient(Locations.AppServiceUrl, new AuthenticationDelegatingHandler());

            // Handle local development, if configured
            if (Locations.AlternateLoginHost != null)
            {
                Client.AlternateLoginHost = new Uri(Locations.AlternateLoginHost);
            }

            // Obtain the Xamarin reference to the platform-specific code
            PlatformProvider = DependencyService.Get<IPlatform>();
            if (PlatformProvider == null)
            {
                throw new InvalidOperationException("No Platform Provider");
            }
        }

        private async Task InitializeAsync()
        {
            if (Client.SyncContext.IsInitialized)
            {
                return;
            }

            var store = new MobileServiceSQLiteStore("tasklist.db");
            store.DefineTable<TodoItem>();
            await Client.SyncContext.InitializeAsync(store);

            TaskTable = Client.GetSyncTable<TodoItem>();
        }

        #region ICloudService Interface
        public async Task DeleteTaskAsync(TodoItem item)
        {
            await InitializeAsync();
            await TaskTable.DeleteAsync(item);
        }

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
            return (Identities.Count > 0) ? Identities[0] : null;
        }

        public async Task<MobileServiceUser> LoginAsync()
        {
            Client.CurrentUser = PlatformProvider.RetrieveTokenFromSecureStore();
            if (Client.CurrentUser != null)
            {
                try
                {
                    var refreshedUser = await Client.RefreshUserAsync();
                    if (refreshedUser != null)
                    {
                        PlatformProvider.StoreTokenInSecureStore(refreshedUser);
                        return refreshedUser;
                    }
                }
                catch (Exception refreshException)
                {
                    Debug.WriteLine($"Refresh User Failure {refreshException.Message}");
                }
            }

            if (Client.CurrentUser != null && !IsTokenExpired(Client.CurrentUser.MobileServiceAuthenticationToken))
            {
                Debug.WriteLine($"Using existing token");
                return Client.CurrentUser;
            }

            await PlatformProvider.LoginAsync(Client);
            if (Client.CurrentUser != null)
            {
                PlatformProvider.StoreTokenInSecureStore(Client.CurrentUser);
                return Client.CurrentUser;
            }

            PlatformProvider.RemoveTokenFromSecureStore();
            return null;
        }

        public async Task LogoutAsync()
        {
            if (Client.CurrentUser == null || Client.CurrentUser?.MobileServiceAuthenticationToken == null)
            {
                return;
            }

            await PlatformProvider.LogoutAsync();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", Client.CurrentUser.MobileServiceAuthenticationToken);
                await httpClient.GetAsync(new Uri($"{Client.MobileAppUri}/.auth/logout"));
            }
            PlatformProvider.RemoveTokenFromSecureStore();
            await Client.LogoutAsync();
        }

        public async Task<TodoItem> ReadTaskAsync(string id)
        {
            await InitializeAsync();
            return await TaskTable.LookupAsync(id);
        }

        public async Task<ICollection<TodoItem>> ReadTasksAsync(int? start, int? count)
        {
            await InitializeAsync();

            if (start != null && count != null)
            {
                return await TaskTable
                    .Skip((int)start)
                    .Take((int)count)
                    .ToListAsync();
            }

            List<TodoItem> allItems = new List<TodoItem>();
            var pageSize = 25;
            var hasMore = true;
            while (hasMore)
            {
                var pageOfItems = await TaskTable.Skip(allItems.Count).Take(pageSize).ToListAsync();
                if (pageOfItems.Count > 0)
                {
                    allItems.AddRange(pageOfItems);
                }
                else
                {
                    hasMore = false;
                }
            }
            return allItems;
        }

        public async Task SyncOfflineCacheAsync()
        {
            await InitializeAsync();

            if (Client.CurrentUser == null || Client.CurrentUser?.MobileServiceAuthenticationToken == null)
            {
                await LoginAsync();
            }

            await Client.SyncContext.PushAsync();
            await TaskTable.PullAsync("incsync_TodoItem", TaskTable.CreateQuery());
        }

        public async Task<TodoItem> UpsertTaskAsync(TodoItem item)
        {
            await InitializeAsync();

            if (item.Id == null)
            {
                await TaskTable.InsertAsync(item);
            }
            else
            {
                await TaskTable.UpdateAsync(item);
            }
            return item;
        }
        #endregion

        private bool IsTokenExpired(string token)
        {
            var jwt = token.Split(new Char[] { '.' })[1].Replace('-','+').Replace('_','/');
            switch (jwt.Length % 4)
            {
                case 0: break;
                case 2: jwt += "=="; break;
                case 3: jwt += "="; break;
                default:
                    throw new ArgumentException("JWT is not valid base64");
            }

            var bytes = Convert.FromBase64String(jwt);
            var jsonObject = JObject.Parse(UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length));
            var exp = Convert.ToDouble(jsonObject["exp"].ToString());

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expiry = epoch.AddSeconds(exp);
            return expiry < DateTime.UtcNow;
        }

        public async Task<StorageTokenViewModel> GetSasTokenAsync()
        {
            var parameters = new Dictionary<string, string>();
            var storageToken = await Client.InvokeApiAsync<StorageTokenViewModel>("GetStorageToken", HttpMethod.Get, parameters);
            return storageToken;
        }
    }
}
