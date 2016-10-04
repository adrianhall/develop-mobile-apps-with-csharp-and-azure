using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using Windows.Security.Credentials;
using Plugin.Media;

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
            // Nothing to do here...
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <summary>
        /// Remove the user records from the secure store
        /// </summary>
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

        /// <summary>
        /// Retrieve the user authentication token from the secure store
        /// </summary>
        /// <returns>The user record</returns>
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

        /// <summary>
        /// Store the MobileServiceUser authentication token into the secure store
        /// </summary>
        /// <param name="user">The user record</param>
        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            PasswordVault.Add(new PasswordCredential(ServiceIdentifier, user.UserId, user.MobileServiceAuthenticationToken));
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
            var dbName = $"{ServiceIdentifier}.db";
            return dbName;
        }
        #endregion
    }
}
