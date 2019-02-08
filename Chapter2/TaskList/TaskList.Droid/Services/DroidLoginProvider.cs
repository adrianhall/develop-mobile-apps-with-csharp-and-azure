using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;
using TaskList.Droid.Services;
using TaskList.Helpers;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace TaskList.Droid.Services
{
    public class DroidLoginProvider : ILoginProvider
    {
        #region ILoginProvider Interface
        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            var accounts = AccountStore.FindAccountsForService("tasklist");
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
            AccountStore.Save(account, "tasklist");
        }

        public void RemoveTokenFromSecureStore()
        {
            var accounts = AccountStore.FindAccountsForService("tasklist");
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    AccountStore.Delete(acct, "tasklist");
                }
            }
        }

        public async Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            #region Azure AD Client Flow
            // var accessToken = await LoginADALAsync();
            //var zumoPayload = new JObject();
            //zumoPayload["access_token"] = accessToken;
            //return await client.LoginAsync("aad", zumoPayload);
            #endregion

            #region Auth0 Client Flow
            // var accessToken = await LoginAuth0Async();
            //var zumoPayload = new JObject();
            //zumoPayload["access_token"] = accessToken;
            //return await client.LoginAsync("auth0", zumoPayload);
            #endregion

            // Server Flow
            return await client.LoginAsync(RootView, "aad");
        }
        #endregion


        public Context RootView { get; private set; }

        public AccountStore AccountStore { get; private set; }

        public void Init(Context context)
        {
            RootView = context;
            AccountStore = AccountStore.Create(context);
        }

        #region Azure AD Client Flow
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
                new PlatformParameters((Activity)RootView));
            return authResult.AccessToken;
        }
        #endregion

        #region Auth0 Client Flow
        public async Task<string> LoginAuth0Async()
        {
            var auth0 = new Auth0.SDK.Auth0Client("shellmonger.auth0.com", "lmFp5jXnwPpD9lQIYwgwwPmFeofuLpYq");
            var user = await auth0.LoginAsync(RootView, scope: "openid email name");
            return user.IdToken;
        }    
        #endregion
    }
}
