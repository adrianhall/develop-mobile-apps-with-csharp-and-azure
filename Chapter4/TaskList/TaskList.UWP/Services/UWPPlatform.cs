using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using Windows.Security.Credentials;

[assembly: Xamarin.Forms.Dependency(typeof(TaskList.UWP.Services.UWPPlatform))]
namespace TaskList.UWP.Services
{
    public class UWPPlatform : IPlatform
    {
        /// <summary>
        /// Identifier to tag the data we store in the secure store
        /// </summary>
        private const string ServiceIdentifier = "thebook";

        /// <summary>
        /// Reference to the secure store for the device
        /// </summary>
        public PasswordVault PasswordVault { get; private set; }

        /// <summary>
        /// Initialize the platform provider
        /// </summary>
        public UWPPlatform()
        {
            PasswordVault = new PasswordVault();
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
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(
                Locations.AppServiceUrl,
                Locations.AadClientId,
                new Uri(Locations.AadRedirectUri),
                new PlatformParameters(PromptBehavior.Auto, false));
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LogoutAsync()
        {
            // Nothing to do here...
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public void RemoveTokenFromSecureStore()
        {
            try
            {
                var acct = PasswordVault.FindAllByResource(ServiceIdentifier).FirstOrDefault();
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

        public MobileServiceUser RetrieveTokenFromSecureStore()
        {
            try
            {
                var acct = PasswordVault.FindAllByResource(ServiceIdentifier).FirstOrDefault();
                if (acct != null)
                {
                    var token = PasswordVault.Retrieve(ServiceIdentifier, acct.UserName).Password;
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
            PasswordVault.Add(new PasswordCredential(ServiceIdentifier, user.UserId, user.MobileServiceAuthenticationToken));
        }
        #endregion
    }
}
