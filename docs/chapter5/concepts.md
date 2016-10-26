Thus far, we've looked at options for communicating with the backend while the client is running.  Nothing happens 
within the mobile client until and unless the mobile client is visible and running.  When the user changes apps, the 
client is suspended or placed on a low priority background thread.  No user interaction is possible.

Most developers have a need to communicate interesting things to the user, which can't happen if the app isn't 
running.  Fortunately, the major mobile platform providers have implemented some form of push notifications.

Push notifications are messages that are sent to your mobile client irrespective of whether the app is running or 
not.  The mobile device will wake up your app to deliver the message.  You have probably seen many examples of push 
notifications in your daily mobile phone usage.  There are several uses, but they fall into two broad areas.  Marketing 
messages are sent to inform the user of the app of something.  Perhaps it's a new version, or a specific promotion for 
your favorite store.  Silent notifications are sent to inform the app that something important has happened.  For 
example, you may want to send a message when a data element has been updated in a table.  Silent notifications are
generally not to be seen by the user.

Delivery of these messages comes with some signficant penalties.  You cannot guarantee the delivery of a message.  The 
user of the device decides whether to accept messages or not.  You cannot guarantee a delivery time, even though most 
messages are delivered within a couple of minutes.  Finally, there is no built in acknowledgement of the message.  You 
have to do something extra in code to ensure the delivery happens.

## How Push Notifications Works

Each platform provider provides their own push notification service.  For example, iOS uses *Apple Push Notification 
Service (APNS)*.  Google uses *Firebase Communications Manager (FCM)*.  This used to be called Google Communications 
Manager or GCM.  It's the same service; just rebranded.  Newer versions of Windows (including Universal Windows) use 
*Windows Notification Service (WNS)* whereas older versions of Windows Phone used *Microsoft Platform Notification 
Service (MPNS)*.  There are other push notification services, for example, for FireOS (run by Amazon) and China (run 
by Baidu).

In all cases, the process is the same:

![][img1]

The mobile device initiates the process, registering with the Platform Notification Service (PNS).  It will receive a 
**Registration ID** in return. The registration ID is specific to an app running on a specific device. Once you have 
the registration ID, you will pass that registration ID to your backend.  The backend will use the registration ID 
when communicating with the PNS to send your app messages.

This is where complexity rears its ugly head.  Without an intervening service, the backend will need to do the 
following:

* Store the registration ID and PNS in a database for later reference.
* When a message is to be sent, lookup the list of registration IDs on a per-PNS basis and send the message in batches.
* Handle retry, incremental back-off, throttling and tracking for each message.
* Deal with registration failure and maintenance of the database.

This is just the start of the functionality.  In general, marketeers will want tracking of the messages (such as how 
many were opened or acted on, what demographics were the opened messages, etc.) and they will want to push to only a 
subset of users, targetted by opt-in lists or other demographic information.  

## Introducing Notification Hubs

Developing a system for pushing notifications to devices is a significant undertaking.  I would not recommend anyone
undertake such a service for their app.  Fortunately, there are a number of services that can do this for you.  Azures 
entry into this space is [Azure Notification Hubs][1].  Notification Hubs (or NH as we will call it) handles all the 
registration and bulk sending logic to allow you to send a single message to multiple recipients without having to 
worry about what platform they are on.  In addition, NH has support for tagging individual device registrations with 
information about the user, groups, or opt-in lists.

Azure Mobile Apps has direct support for Notification Hubs within the client SDK and Azure App Service has a 
registration service built right in for NH, allowing you to easily integrate your mobile app with the facilities 
that NH provides.

!!! tip
    You do not have to run Azure Mobile Apps or Azure App Service to use Notification Hubs.  You do need to have a 
    registration service somewhere.  However, Notification Hubs is a standalone service.

In this chapter, we will be expanding our simple Task list app to support push notifications using Notification Hubs
and demonstrate a few different scenarios on handling push notifications on the backend.

## Configuring Notification Hubs

Our first step is to configure our backend.  Thus far, we have implemented an Azure App Service with a SQL Azure
database, and that is our starting point again.  To those resources, we will add the Notification Hub, which starts
just like the addition of any other resource:

* Log into the [Azure Portal].
* Click on the **+ NEW** button in the top right corner.
* Select or search for **Notification Hub**.
* Click on **Create**.

    ![][img2]

* 

## Configuring Push Registration



<!-- Images -->
[img1]: img/push-architecture.PNG

<!-- Links -->
[1]: https://azure.com/something-something-something
