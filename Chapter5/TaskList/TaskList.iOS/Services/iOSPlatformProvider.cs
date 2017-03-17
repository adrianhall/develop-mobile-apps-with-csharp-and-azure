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
            //var accounts = AccountStore.FindAccountsForService("tasklist");
            //if (accounts != null)
            //{
            //    foreach (var acct in accounts)
            //    {
            //        string token;
			//
            //        if (acct.Properties.TryGetValue("token", out token))
            //        {
            //            return new MobileServiceUser(acct.Username)
            //            {
            //                MobileServiceAuthenticationToken = token
            //            };
            //        }
            //    }
            //}
            return null;
        }

        public void StoreTokenInSecureStore(MobileServiceUser user)
        {
            //var account = new Account(user.UserId);
            //account.Properties.Add("token", user.MobileServiceAuthenticationToken);
            //AccountStore.Save(account, "tasklist");
        }

        public void RemoveTokenFromSecureStore()
        {
            //var accounts = AccountStore.FindAccountsForService("tasklist");
            //if (accounts != null)
            //{
            //   foreach (var acct in accounts)
            //    {
            //        AccountStore.Delete(acct, "tasklist");
            //    }
            //}
        }

		public async Task<MobileServiceUser> LoginAsync(MobileServiceClient client)
		{
			return await client.LoginAsync(RootView, "aad");
		}

		public async Task RegisterForPushNotifications(MobileServiceClient client)
		{
			if (AppDelegate.PushDeviceToken != null)
			{
				try
				{
					var registrationId = AppDelegate.PushDeviceToken.Description
						.Trim('<', '>').Replace(" ", string.Empty).ToUpperInvariant();
					var installation = new DeviceInstallation
					{
						InstallationId = client.InstallationId,
						Platform = "apns",
						PushChannel = registrationId
					};
					// Set up tags to request
					installation.Tags.Add("topic:Sports");
					// Set up templates to request
					PushTemplate genericTemplate = new PushTemplate
					{
						Body = @"{""aps"":{""alert"":""$(message)"",""picture"":""$(picture)""}}"
					};
					installation.Templates.Add("genericTemplate", genericTemplate);
					// Register with NH
					var recordedInstallation = await client.InvokeApiAsync<DeviceInstallation, DeviceInstallation>(
						$"/push/installations/{client.InstallationId}",
						installation,
						HttpMethod.Put,
						new Dictionary<string, string>());
					System.Diagnostics.Debug.WriteLine("Completed NH Push Installation");
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.Fail($"[iOSPlatformProvider]: Could not register with NH: {ex.Message}");
				}
			}
			else
			{
				System.Diagnostics.Debug.WriteLine($"[iOSPlatformProvider]: Are you running in a simulator?  Push Notifications are not available");
			}
		}
	}
}