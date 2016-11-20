using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.iOS.Services;
using UIKit;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(iOSPlatformProvider))]
namespace TaskList.iOS.Services
{
    public class iOSPlatformProvider : IPlatformProvider
    {
        public UIViewController RootView => UIApplication.SharedApplication.KeyWindow.RootViewController;

        public AccountStore AccountStore { get; private set; }

        public iOSPlatformProvider()
        {
            AccountStore = AccountStore.Create();
        }

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
            var accessToken = await LoginADALAsync();
            var zumoPayload = new JObject();
            zumoPayload["access_token"] = accessToken;
            return await client.LoginAsync("aad", zumoPayload);
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

		public async Task RegisterForPushNotifications(MobileServiceClient client)
		{
			if (AppDelegate.PushDeviceToken != null)
			{
				try
				{
					var installation = new DeviceInstallation
					{
						InstallationId = client.InstallationId,
						Platform = "apns",
						PushChannel = AppDelegate.PushDeviceToken.ToString()
					};
					// Set up tags to request
					installation.Tags.Add("topic:Sports");
					// Set up templates to request
					PushTemplate genericTemplate = new PushTemplate
					{
						Body = "{\"aps\":{\"alert\":\"$(messageParam)\"}}"
					};
					// Register with NH
					var response = await client.InvokeApiAsync<DeviceInstallation, DeviceInstallation>(
						$"/push/installations/{client.InstallationId}",
						installation,
						HttpMethod.Put,
						new Dictionary<string, string>());
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.Fail($"[iOSPlatformProvider]: Could not register with NH: {ex.Message}");
				}
			}
		}
		#endregion
	}
}