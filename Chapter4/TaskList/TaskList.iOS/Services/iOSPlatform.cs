using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using Xamarin.Auth;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(TaskList.iOS.Services.iOSPlatform))]
namespace TaskList.iOS.Services
{
    public class iOSPlatform : IPlatform
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
        /// Reference to the root view for the application
        /// </summary>
        public UIViewController RootView => UIApplication.SharedApplication.KeyWindow.RootViewController;

        /// <summary>
        /// Initializer for this platform provider
        /// </summary>
        public iOSPlatform()
        {
            AccountStore = AccountStore.Create();
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
                new PlatformParameters(RootView));
            return authResult.AccessToken;
        }

        #region IPlatform Interface
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
        #endregion
    }
}
