using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Xamarin.Forms;
using TaskList.ViewModels;
using TaskList.Models;

namespace TaskList.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public static NSData PushDeviceToken { get; private set; } = null;

		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{
			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			global::Xamarin.Forms.Forms.Init();
			LoadApplication(new App());

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
					UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
					new NSSet());
				UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
				UIApplication.SharedApplication.RegisterForRemoteNotifications();
			}

			return base.FinishedLaunching(uiApplication, launchOptions);
		}

		/// <summary>
		/// Called when the push notification system is registered
		/// </summary>
		/// <param name="application">Application.</param>
		/// <param name="deviceToken">Device token.</param>
		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			AppDelegate.PushDeviceToken = deviceToken;
		}

		/// <summary>
		/// Handler for Push Notifications
		/// </summary>
		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{
			ProcessNotification(userInfo, false);
		}

		private void ProcessNotification(NSDictionary options, bool fromFinishedLoading)
		{
			if (!(options != null && options.ContainsKey(new NSString("aps"))))
			{
				// Short circuit - nothing to do
				return;
			}

			NSDictionary aps = options.ObjectForKey(new NSString("aps")) as NSDictionary;

			if (!fromFinishedLoading)
			{
				var alertString = GetStringFromOptions(aps, "alert");
				// Manually show an alert
				if (!string.IsNullOrEmpty(alertString))
				{
					var pictureString = GetStringFromOptions(aps, "picture");

					UIAlertView alertView = new UIAlertView(
						"TaskList",
						alertString,
						null,
						NSBundle.MainBundle.LocalizedString("Cancel", "Cancel"),
						NSBundle.MainBundle.LocalizedString("OK", "OK")
					);
					alertView.Clicked += (sender, args) =>
					{
						if (args.ButtonIndex != alertView.CancelButtonIndex)
						{
							if (!string.IsNullOrEmpty(pictureString))
							{
								App.Current.MainPage = new NavigationPage(new Pages.PictureView(pictureString));
							}
						}
					};
					alertView.Show();
				}

				var opString = GetStringFromOptions(aps, "op");
				if (!string.IsNullOrEmpty(opString) && opString.Equals("sync"))
				{
					var syncMessage = new PushToSync()
					{
						Table = GetStringFromOptions(aps, "table"),
						Id = GetStringFromOptions(aps, "id")
					};
					MessagingCenter.Send<PushToSync>(syncMessage, "ItemsChanged");
				}
			}
		}

		private string GetStringFromOptions(NSDictionary options, string key)
		{
			string v = string.Empty;
			if (options.ContainsKey(new NSString(key)))
			{
				v = (options[new NSString(key)] as NSString).ToString();
			}
			return v;
		}
	}
}
