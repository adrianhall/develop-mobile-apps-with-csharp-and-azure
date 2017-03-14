Push notifications for Apple devices is handled by _Apple Push Notification Service_ or APNS.  APNS is certificate based, rather than secret based as is the case with FCM.  You will find that there are two certificates - a test certificate that is used for test devices, and a production certificate that is used for production devices.  You can use a common certificate for both (a so-called Universal Certificate).  However, you must ensure that you use the appropriate endpoints - test or production.

It is imperitive that you use a Mac for this configuration.  You will be using Apple native tools to generate certificates and the process of configuring the APNS gateway is made easier by using XCode tools.  You can do certain things on a PC (like editing the plist files appropriately), but you will end up spending a significant amount of time on the Mac.  As a result, I'm going to do this entire section on a Mac.

If you have not done so already, read through the [Android Push](./android.md) section to get all the code for the shared project - it won't be repeated in this section.

## Registering with APNS

Registering with APNS is a multi-step process:

1.  Register an App ID for your app, and select Push Notifications as a capability.
2.  Create an appropriate certificate for the push channel (either a Development or Distribution certificate).
3.  Configure Notification Hubs to use APNS.
4.  Configure your application to support Push Notifications.
5.  Add code for handling push notifications to your app.

Let's cover each one in turn:

### Register an App ID for your app

Once you get to adding push notifications to your application, you are going to need that full developers license from Apple.  You need to work with real devices and that means you need code signing certificates on your mac.  If you have not spent the cash for the Apple Developers program, then you will probably find you need to at this point.

Registering an App ID is handled on the [Apple Developer Portal][1].  Apple does a good job of [documenting the process][2], so these instructions are duplicative of the instructions that Apple provides.

1. Go to the [Apple Developer Portal][1] and log in with your Apple developer ID.
2. In the left-hand menu, click **Certificates, IDs & Profiles**.
3. In the left-hand menu, **Identifiers**, click **App IDs**.
4. Click the **+** button in the top right corner.
5. You can pick either a wildcard App ID or an explicit App ID.  I'm going to use an Explicit App ID for this app.
6. Fill in the form:
    * The App ID Description is not used and can be set to anything (subject to validation rules)
    * Choose an **Explicit App ID** for this app.  (The alternative is a wildcard App ID, which isn't covered here).
    * Enter the App ID suffix according to the rules.  I used `com.shellmonger.tasklist`.

        ![][img1]

    * Select **Push Notifications** in the **App Services** section.

        ![][img2]

7. Click **Continue** when the form is complete.
8. Make a note of the **Identifier** in the next screen, then click **Register**.
9. Click **Done**.

Note that the Push Notifications capability will be listed as _Configurable_ until you create a certificate that is used for push notifications.  Once that happens, the capability will be listed as _Enabled_.

### Create a certificate for the push channel

I mentioned earlier that APNS is certificate based.  That means that you need to generate an SSL certificate to fully configure push notifications:

1.  Staying in **Certificates, Identifiers & Profiles**, click *All** under the **Certificates** heading in the left hand menu.
2.  Click on the **+** button in the top right corner.
3.  Select the **Apple Push Notification service SSL (Sandbox & Production)**

    ![][img3]

4.  Click **Continue**.
5.  Select the App ID you just created from the list, then click **Continue**.
6.  Follow the on-screen instructions for creating a Certificate Signing Request (CSR).

    ![][img4]

7.  Once you have generated the CSR, click **Continue** in the browser.
8.  Select the CSR you just generated using the **Choose File** button, then click **Continue**.
9.  Click **Download** to download the resulting certificate.
10. Click **Done** when the download is complete.
11. Find your downloaded certificate and double-click on it to import it into Keychain Access

Your certificate will also appear in the **Certificates** > **All** list within the Apple Developer console.

### Configure Notification Hubs

Notification Hubs requires you to upload the certificate as a .p12 (PKCS#12) file.  To generate this file:

1. Open Keychain Access.
2. Select **My Certificates** from the left hand menu
3. Right click on the certificate you just generated and select **Export...**.
4. Select **Personal Information Exchange (.p12)** as the type and give it a name and location.
5. Click **Save**.
6. Enter a password (twice) to protect the certificate.
7. Click **OK**.

Now we can upload the certificate to Azure:

1. Open and log into the [Azure portal].
2. Select **Notification Hubs**, then the notification hub that is connected to your mobile backend.
3. Click **Push Notification Services**, then select **Apple (APNS)**.
4. Click **+ Upload Certificate**.
5. Fill in the form:
    * Select the .p12 file you just created.
    * Enter the password that you entered to secure the .p12 file.
    * Select **Sandbox** (probably) or **Production** as appropriate.

    ![][img5]

6. Click **OK**.

It's important to figure out whether you are operating in the _Sandbox_ (Development) or _Production_ mode.  During development, it's likely that your device will be registered on the Apple Developer console and you will be operating in the sandbox.  Any device not listed with the developer console is considered "production".  You must update the certificate to a production certificate and specify the production mode when you release your app.

Apple APNS provides two endpoints for pushing notifications.  If you use the wrong one, then APNS will return an error code.  This will cause Notification Hubs to delete the registration and your push will fail.

### Configure your application

Before we start with code, you will want a _Provisioning Profile_.  This small file is key to being able to use push notifications on your device.  You **MUST** have a real device at this point.  The easiest way for this to happen is to plug the iPhone or iPad that you want to use into your development system.  Once your device is recognized by iTunes, start XCode and look under **Product** > **Destination**.  You should see the device listed there.  For more information on creating a Provisioning Profile, see the [Apple documentation][4].

Moving on to our code, we need to configure the iOS project for push notifications.  This involves configuring the Bundle ID and enabling certain configuration settings required for handling push notifications.  Start by loading your project in Visual Studio for Mac.

1. Expand the **TaskList.iOS** project and open the **Info.plist** file.
2. In the **Identity** section, fill in the **Bundle ID**.  It must match the App ID Suffix that you set earlier.

    ![][img6]

3. Scroll down until you see **Background Modes**.  Check the **Enable Background Modes** checkbox.
4. Check the **Remote notifications** checkbox.

    ![][img7]

5. Right-click on the **TaskList.iOS** project, then select **Options**.
6. Click **iOS Bundle Signing** in the left hand menu.
7. Select your Signing Identity and Provisioning Profile.

!!! tip "Provisioning Profiles are frustrating"
    If you find yourself going round and round in circles on getting the signing certificate and provisioning profile right, you are not alone.  This is possibly one of the most frustrating pieces of iOS development in general.  See this [Xamarin Forums post][5] for a good list of details.

### Code the push handler

The push handler is coded in the `AppDelegate.cs` file.  Unlike other platforms (like Android), you don't have to write code to define the push handler.  It's always in the same place.  Add the following code to the `AppDelegate.cs` file:

```csharp
    public static NSData PushDeviceToken { get; private set; } = null;

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
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

    public override void DidReceiveRemoteNotification(UIApplication application,
        NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
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
```

The `NSDictionary`, `NSData`, and `NSString` classes are part of the iOS programming model and do exactly what you would expect them to do.  The `UIAlertView` class provides a standard alert.  We need to add a little bit of code to the `FinishedLaunching()` method to send the registration request to APNS.  When the response is received, the `RegisteredForRemoteNotifications()` method is called.  Finally, the `DidReceiveRemoteNotification()` method is called whenever a remote push notification is received.

!!! tip "Call common code for push notifications"
    One of the great things about Xamarin Forms is that it is cross-platform.  However, that all breaks down when you move to push notifications.  One of the things you can do is to use the push handler to generate a model and then pass that model to a method in your PCL project.  This allows you to express the differences clearly and yet still do the majority of the logic in a cross-platform manner.

## Registering with Azure Mobile Apps

As with Android, I recommend using a `HttpClient` for registering with Notification Hubs via the Azure Mobile Apps Push handler.  Here is the code that does basically the same thing as the Android version from the `Services\iOSPlatformProvider.cs` file:

```csharp
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
                    Body = "{\"aps\":{\"alert\":\"$(messageParam)\"}}"
                };
                installation.Templates.Add("genericTemplate", genericTemplate);

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
```

In this case, we don't have a service class to deal with - the iOS AppDelegate does all the work for us.  The registration Id is stored in the AppDelegate once registered, but needs to be decoded (which is relatively simple).  Similar to the Android version, we make the template we are using match what we are expecting within our push handler.

!!! tip "Receiving Notifications in the background"
    If you want your app to be notified when a notification is received when your app is in the background, you need to set the Background Fetch capability and your payload should include the key `content-available` with a value of 1 (true).  You can add this to the Body of the template in the above sample.  iOS will wake up the app and you will have 30 seconds to fetch any information you might need to update.  Check [the documentation][6] for more details.

## Testing Notifications

Our final step is to test the whole process.  As with Android, there are two tests we need to perform.  The first is to ensure that a registration happens when we expect it to.  In the case of our app, that happens immediately after the authentication.  There is no Notifications Hub registration monitor in Visual Studio for Mac, so we have to get that information an alternate way, by querying the hub registration endpoint.  I've written [a script] for this purpose.  To install:

* Install [NodeJS].
* Go to the [tools] directory on the books GitHub repository.
* Run `npm install`.

To use, you will need the endpoint for your notification hub namespace.

* Log onto the [Azure portal].
* Open your Notification Hub namespace.
* Click **Access Policies**.
* Copy the connection string of the `RootManagedSharedAccessKey` (which is probably the only policy you have).

You can now use the program using:

```text
node get_nh_registrations.js -c '<your connection string>' -h <your hub name>
```

You will need to put the connection string in quotes generally.  For example:

```text
node .\get_nh_registrations.js -c 'Endpoint=sb://zumobook-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=
****c9VoZHtxSGliSIhH5EEuar1B/jsrgTQTHOTA=' -h zumobook-hub
```

The output will look something like the following:

```text
Type:         APNS (Template)
Id:           219869525209729025-4738338868778066550-1
Device Token: 681F6BB012C62A61AA2185A676B23907A5FEFE9268283DD226B08B5F0336A552
Tag:          topic:Sports
Tag:          _UserId:a9650e1c4d3268ec912f4d9ca6d1d933
Tag:          photoadrian@outlook.com
Tag:          $InstallationId:{d7323fe4-64bb-4d99-a6cc-e7690032350f}
Expires:      9999-12-31T23:59:59.9999999Z
```

Note that this script does not deal with "continuation tokens", so it can only return the first page of information.  This is generally suitable for testing purposes.

We can also send a test message for push notifications.  This can be done via the Azure Portal.

1. Log onto the [Azure portal].
2. Find the Hub resource for your connected Notification Hub and open it.
3. Click **Test Send**.
4. Select **Apple** as the Platform, then click on **Send**.

![][img9]

Your device should also receive the push notification and display an alert.  You can also do a test send to an explicit tag.  This can narrow the test send to just one device if necessary.  To send to a specific device, you need to know the installation ID of the registration.

## Common Problems

As you might expect, there is plenty to go wrong here.  The majority of the issues come down to the fact that there are two endpoints on APNS - a Sandbox (or Developer) endpoint and a Production endpoint.  If you are using the wrong endpoint, the notification hub will receive an error.  If the notification hub receives an error from the APNS endpoint, it will remove the registration causing the error.  This manifests itself in two ways.  Firstly, your device will not receive the push notification.  Secondly, the registration will be removed from the list of valid registrations, causing you to think that the device has not been registered.

This has not been made easy by the fact that Apple has combined the certificates needed to push into a single certificate for both Sandbox and Production use cases.  To correct this issue, ensure the notifiction hub is set up with the appropriate endpoint - Sandbox or Production.

Next you can move onto [Windows Push](./windows.md) or skip to the [Recipes Section](./recipes.md).

<!-- Images -->
[img1]: ./img/push-ios-1.PNG
[img2]: ./img/push-ios-2.PNG
[img3]: ./img/push-ios-3.PNG
[img4]: ./img/push-ios-4.PNG
[img5]: ./img/push-ios-5.PNG
[img6]: ./img/push-ios-6.PNG
[img7]: ./img/push-ios-7.PNG
[img8]: ./img/push-ios-8.PNG
[img9]: ./img/push-ios-9.PNG

<!-- Links -->
[Azure portal]: https;//portal.azure.com/
[NodeJS]: https://nodejs.org/en/download/
[tools]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/tree/master/tools
[1]: http://developer.apple.com/account
[2]: https://developer.apple.com/library/content/documentation/IDEs/Conceptual/AppDistributionGuide/MaintainingProfiles/MaintainingProfiles.html#//apple_ref/doc/uid/TP40012582-CH30-SW991
[3]: https://developer.apple.com/library/content/documentation/IDEs/Conceptual/AppDistributionGuide/MaintainingProfiles/MaintainingProfiles.html
[4]: https://developer.apple.com/library/content/documentation/IDEs/Conceptual/AppDistributionGuide/MaintainingProfiles/MaintainingProfiles.html#//apple_ref/doc/uid/TP40012582-CH30-SW10
[5]: https://forums.xamarin.com/discussion/13497
[6]: https://developer.xamarin.com/guides/ios/application_fundamentals/backgrounding/part_3_ios_backgrounding_techniques/updating_an_application_in_the_background/
