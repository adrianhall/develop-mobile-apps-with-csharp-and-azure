!!! warn "Visual Studio for Mac Use"
    I'm going to be using Visual Studio for Mac for screen shots in this section.  You can also use Xamarin Studio 
    or Visual Studio on the PC and then compile using a Mac Build Agent.  The same code will work.  Some terminology
    differences may exist and locations may differ.

Push notifications for Apple devices is handled by _Apple Push Notification Service_ or APNS.  APNS is certificate
based, rather than secret based as is the case with GCM and FCM.  You will find that there are two certificates -
a test certificate that is used for test devices, and a production certificate that is used for production devices.
You can use a common certificate for both (a so-called Universal Certificate).  However, you must ensure that you
use the appropriate endpoints - test or production.

It is imperitive that you use a Mac for this configuration.  You will be using Apple native tools to generate
certificates and the process of configuring the APNS gateway is made easier by using XCode tools.  You can do
certain things on a PC (like editing the plist files appropriately), but you will end up spending a significant
amount of time on the Mac.  As a result, I'm going to do this entire section on a Mac.

## Registering with APNS

Registering with APNS is a multi-step process:

1.  Register an App ID for your app, and select Push Notifications as a capability.
2.  Create an appropriate certificate for the push channel (either a Development or Distribution certificate).
3.  Configure Notification Hubs to use APNS.
4.  Configure your application to support Push Notifications.
5.  Add code for handling push notifications to your app.

Let's cover each one in turn:

### Register an App ID for your app

Once you get to adding push notifications to your application, you are going to need that full developers license
from Apple.  You need to work with real devices and that means you need code signing certificates on your mac.  If
you have not spent the cash for the Apple Developers program, then you will probably find you need to at this 
point.

Registering an App ID is handled on the [Apple Developer Portal][1].  Apple does a good job of [documenting the process][2],
so these instructions are duplicative of the instructions that Apple provides.

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

Note that the Push Notifications capability will be listed as _Configurable_ until you create a certificate that
is used for push notifications.  Once that happens, the capability will be listed as _Enabled_.

### Create a certificate for the push channel

I mentioned earlier that APNS is certificate based.  That means that you need to generate an SSL certificate to
fully configure push notifications:

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

It's important to figure out whether you are operating in the _Sandbox_ (Development) or _Production_ mode.  During 
development, it's likely that your device will be registered on the Apple Developer console and you will be 
operating in the sandbox.  Any device not listed with the developer console is considered "production".  You 
must update the certificate to a production certificate and specify the production mode when you release your app.  

Apple APNS provides two endpoints for pushing notifications.  If you use the wrong one, then APNS will return an
error code.  This will cause Notification Hubs to delete the registration and your push will fail.

### Configure your application

Before we start with code, you will want a _Provisioning Profile_.  This small file is key to being able to
use push notifications on your device.  You **MUST** have a real device at this point.  The easiest way for
this to happen is to plug the iPhone or iPad that you want to use into your development system.  Once your
device is recognized by iTunes, start XCode and look under **Product** > **Destination**.  You should see
the device listed there.

!!! warn "INCOMPLETE SECTION"
    This section is incomplete.

Moving on to our code, we need to configure the iOS project for push notifications.  This involves configuring
the Bundle ID and enabling certain configuration settings required for handling push notifications.  Start by
loading your project.  

1. Expand the **TaskList.iOS** project and open the **Info.plist** file.
2. In the **Identity** section, fill in the **Bundle ID**.  It must match the App ID Suffix that you set earlier.

    ![][img6]

3. Scroll down until you see **Background Modes**.  Check the **Enable Background Modes** checkbox.
4. Check the **Remote notifications** checkbox.

    ![][img7]

5. Right-click on the **TaskList.iOS** project, then select **Options**.
6. Click **iOS Bundle Signing** in the left hand menu.
7. Select your Signing Identity and Provisioning Profile.

!!! warn "INCOMPLETE SECTION"
    This section is incomplete.

### Code the push handler

## Registering with Azure Mobile Apps

## Receiving a Notifications

<!-- Images -->
[img1]: ./img/push-ios-1.PNG
[img2]: ./img/push-ios-2.PNG
[img3]: ./img/push-ios-3.PNG
[img4]: ./img/push-ios-4.PNG
[img5]: ./img/push-ios-5.PNG
[img6]: ./img/push-ios-6.PNG
[img7]: ./img/push-ios-7.PNG

<!-- Links -->
[Azure portal]: https;//portal.azure.com/
[1]: http://developer.apple.com/account
[2]: https://developer.apple.com/library/content/documentation/IDEs/Conceptual/AppDistributionGuide/MaintainingProfiles/MaintainingProfiles.html#//apple_ref/doc/uid/TP40012582-CH30-SW991
[3]: https://developer.apple.com/library/content/documentation/IDEs/Conceptual/AppDistributionGuide/MaintainingProfiles/MaintainingProfiles.html