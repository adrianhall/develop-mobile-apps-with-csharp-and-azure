using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.UWP.Services;
using Windows.Security.Credentials;

[assembly: Xamarin.Forms.Dependency(typeof(UWPLoginProvider))]
namespace TaskList.UWP.Services
{
    public class UWPLoginProvider : ILoginProvider
    {
        public PasswordVault PasswordVault { get; private set; }

        public UWPLoginProvider()
        {
            PasswordVault = new PasswordVault();
        }

        #region ILoginProvider Interface
        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            try
            {
                // Check if the token is available within the password vault
                var acct = PasswordVault.FindAllByResource("tasklist").FirstOrDefault();
                if (acct != null)
                {
                    var token = PasswordVault.Retrieve("tasklist", acct.UserName).Password;
                    if (token != null && token.Length > 0)
                    {
                        return new MobileServiceUser(acct.UserName)
                        {
                            MobileServiceAuthenticationToken = token
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving existing token: {ex.Message}");
            }
            return null;
        }

        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            PasswordVault.Add(new PasswordCredential("tasklist", user.UserId, user.MobileServiceAuthenticationToken));
        }

        public void RemoveTokenFromSecureStore()
        {
            try
            {
                // Check if the token is available within the password vault
                var acct = PasswordVault.FindAllByResource("tasklist").FirstOrDefault();
                if (acct != null)
                {
                    PasswordVault.Remove(acct);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving existing token: {ex.Message}");
            }
        }

        public async Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
        {
            #region Azure AD Client Flow
            // Client Flow
            // var accessToken = await LoginADALAsync();
            // var zumoPayload = new JObject();
            // zumoPayload["access_token"] = accessToken;
            // return await client.LoginAsync("aad", zumoPayload);
            #endregion

            // Server-Flow Version
            return await client.LoginAsync("aad");
        }
        #endregion

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
                new PlatformParameters(PromptBehavior.Auto, false));
            return authResult.AccessToken;
        }

        #endregion
    }
}
