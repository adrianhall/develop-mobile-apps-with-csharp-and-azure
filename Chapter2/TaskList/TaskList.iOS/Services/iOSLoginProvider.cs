using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facebook.LoginKit;
using Foundation;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.iOS.Services;
using UIKit;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(iOSLoginProvider))]
namespace TaskList.iOS.Services
{
    public class iOSLoginProvider : ILoginProvider
    {
        public UIViewController RootView => UIApplication.SharedApplication.KeyWindow.RootViewController;

        public AccountStore AccountStore { get; private set; }

        public iOSLoginProvider()
        {
            AccountStore = AccountStore.Create();
        }

        public async Task LoginAsync(MobileServiceClient client)
        {
            // Check if the token is available within the key store
            var accounts = AccountStore.FindAccountsForService("tasklist");
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    string token;

                    if (acct.Properties.TryGetValue("token", out token))
                    {
                        if (!IsTokenExpired(token))
                        {
                            client.CurrentUser = new MobileServiceUser(acct.Username);
                            client.CurrentUser.MobileServiceAuthenticationToken = token;
                            return;
                        }
                    }
                }
            }

            #region Azure AD Client Flow
            // var accessToken = await LoginADALAsync();
            //var zumoPayload = new JObject();
            //zumoPayload["access_token"] = accessToken;
            //await client.LoginAsync("aad", zumoPayload);
            #endregion

            #region Auth0 Client Flow
            // var accessToken = await LoginAuth0Async();
            //var zumoPayload = new JObject();
            //zumoPayload["access_token"] = accessToken;
            //await client.LoginAsync("auth0", zumoPayload);
            #endregion

            #region Facebook Client Flow
            // var accessToken = await LoginFacebookAsync();
            //var zumoPayload = new JObject();
            //zumoPayload["access_token"] = accessToken;
            //await client.LoginAsync("facebook", zumoPayload);
            #endregion

            // Server Flow
            await client.LoginAsync(RootView, "aad");

            // Store the new token within the store
            var account = new Account(client.CurrentUser.UserId);
            account.Properties.Add("token", client.CurrentUser.MobileServiceAuthenticationToken);
            AccountStore.Save(account, "tasklist");
        }

        bool IsTokenExpired(string token)
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
            return (expire < DateTime.UtcNow);
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

        #region Facebook Client Flow
        private TaskCompletionSource<string> fbtcs;

        public async Task<string> LoginFacebookAsync()
        {
            fbtcs = new TaskCompletionSource<string>();
            var loginManager = new LoginManager();

            loginManager.LogInWithReadPermissions(new[] { "public_profile" }, RootView, LoginTokenHandler);
            return await fbtcs.Task;
        }

        private void LoginTokenHandler(LoginManagerLoginResult loginResult, NSError error)
        {
            if (loginResult.Token != null)
            {
                fbtcs.TrySetResult(loginResult.Token.TokenString);
            }
            else
            {
                fbtcs.TrySetException(new Exception("Facebook Client Flow Login Failed"));
            }
        }
        #endregion

        #region Auth0 Client Flow
        public async Task<string> LoginAuth0Async()
        {
            var auth0 = new Auth0.SDK.Auth0Client(
                "shellmonger.auth0.com",
                "lmFp5jXnwPpD9lQIYwgwwPmFeofuLpYq");
            var user = await auth0.LoginAsync(RootView, scope: "openid email name");
            return user.IdToken;
        }
        #endregion
    }
}