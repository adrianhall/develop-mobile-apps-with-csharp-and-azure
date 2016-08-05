using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Facebook.LoginKit;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Droid.Services;
using TaskList.Helpers;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace TaskList.Droid.Services
{
    public class DroidLoginProvider : ILoginProvider
    {
        public Context Context { get; }

        public void Init(Context context)
        {
            Context = context;
        }



        public async Task LoginAsync(MobileServiceClient client)
        {
            // Client Flow
            // var accessToken = await LoginADALAsync();
            var accessToken = await LoginAuth0Async();

            var zumoPayload = new JObject();
            zumoPayload["access_token"] = accessToken;
            // await client.LoginAsync("aad", zumoPayload);
            await client.LoginAsync("auth0", zumoPayload);

            // Server Flow
            // await client.LoginAsync(Context, "aad");
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
                new PlatformParameters((Activity)context));
            return authResult.AccessToken;
        }
        #endregion

        #region Auth0 Client Flow
        public async Task<string> LoginAuth0Async()
        {
            var auth0 = new Auth0.SDK.Auth0Client(
                "shellmonger.auth0.com",
                "lmFp5jXnwPpD9lQIYwgwwPmFeofuLpYq");
            var user = await auth0.LoginAsync(Context, scope: "openid email name");
            return user.IdToken;
        }
        #endregion
    }
}
