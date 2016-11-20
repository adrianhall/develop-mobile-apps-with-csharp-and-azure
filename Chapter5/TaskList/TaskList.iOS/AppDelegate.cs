using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

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
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
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
			NSDictionary aps = userInfo.ObjectForKey(new NSString("aps")) as NSDictionary;

			// The aps is a dictionary with the template values in it
			// You can adjust this section to do whatever you need to with the push notification

			string alert = string.Empty;
			if (aps.ContainsKey(new NSString("alert")))
				alert = (aps[new NSString("alert")] as NSString).ToString();

			//show alert
			if (!string.IsNullOrEmpty(alert))
			{
				UIAlertView avAlert = new UIAlertView("Notification", alert, null, "OK", null);
				avAlert.Show();
			}

			// End of the user configurable piece
		}
    }
}
