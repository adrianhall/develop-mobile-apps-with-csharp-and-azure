using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Droid.Services;
using TaskList.Helpers;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace TaskList.Droid.Services
{
    public class DroidLoginProvider : ILoginProvider
    {
        public Context RootView { get; private set; }

        public AccountStore AccountStore { get; private set; }

        public void Init(Context context)
        {
            RootView = context;
            AccountStore = AccountStore.Create(context);
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
