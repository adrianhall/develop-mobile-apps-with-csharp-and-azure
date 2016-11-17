!!! warn Visual Studio for Mac Use
    I'm going to be using Visual Studio for Mac in this section.  You can also use Xamarin Studio or Visual Studio
    on the PC and then compile using a Mac Build Agent.  The same code will work.

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
3.  Configure your application to support Push Notifications.
4.  Add code for handling push notifications to your app.

Let's cover each one in turn:

### Register an App ID for your app

### Create a certificate for the push channel

### Configure your application

### Code the push handler

## Registering with Azure Mobile Apps

## Receiving a Notifications

