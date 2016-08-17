using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.iOS.Services;
using UIKit;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(iOSPlatform))]
namespace TaskList.iOS.Services
{
    public class iOSPlatform : IPlatform
    {
        const string ServiceIdentifier = "chapter3-tasklist";

        public UIViewController RootView => UIApplication.SharedApplication.KeyWindow.RootViewController;

        public AccountStore AccountStore { get; private set; }

        public iOSPlatform()
        {
            AccountStore = AccountStore.Create();
        }

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
                        return new MobileServiceUser(acct.Username)
                        {
                            MobileServiceAuthenticationToken = token
                        };
                    }
                }
            }
            return null;
        }

        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            var account = new Account(user.UserId);
            account.Properties.Add("token", user.MobileServiceAuthenticationToken);
            AccountStore.Save(account, ServiceIdentifier);
        }

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

        public async Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            var accessToken = await LoginADALAsync();

            var zumoPayload = new JObject();
            zumoPayload["access_token"] = accessToken;
            return await client.LoginAsync("aad", zumoPayload);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LogoutAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Do nothing
        }

        #region Azure AD Client Flow
        /// <summary>
        /// Login via ADAL
        /// </summary>
        /// <returns>(async) token from the ADAL process</returns>
        public async Task<string> LoginADALAsync()
        {
            Uri returnUri = new Uri(Locations.AadRedirectUri);

            var authContext = new AuthenticationContext(Locations.AadAuthority);
            if (authContext.TokenCache.ReadItems().Count() > 0)
            {
                authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
            }

            var authResult = await authContext.AcquireTokenAsync(
                Locations.AppServiceUrl, /* The resource we want to access  */
                Locations.AadClientId,   /* The Client ID of the Native App */
                returnUri,               /* The Return URI we configured    */
                new PlatformParameters(RootView));
            return authResult.AccessToken;
        }
        #endregion
    }
}
