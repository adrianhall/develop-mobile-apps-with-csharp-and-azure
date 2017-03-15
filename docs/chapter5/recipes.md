This section is dedicated to exploring various common recipes for push notifications and how we can achieve those recipes through cross-platform code.

!!! warn "Incomplete Section"
    This section is incomplete and still in progress.  Please do not rely on any information within this section.

## Marketing Push

The most common requirement for push notifications is to alert users of a special offer or other marketing information.  The general idea is that the marketing person will create a "campaign" that includes a push notification.  When the user receives the push notification, they will accept it.  If a user accepts the push notification, the mobile app will deep-link into a specific page and store the fact that the user viewed the page within the database.

To implement this sort of functionality within a cross-platform application, we need to implement **Templates**.  We gave demonstrations of the implementation of the templates while we were discussing the various platform implementations.  However, we didn't actually use them.  A template is provides by the mobile client when registering.  Let's take a look at a typical template as implemented by each platform:

**Android**:

```text
{
    "data": {
        "message": "$(message)",
        "picture": "$(picture)",
        "view": "$(viewid)"
    }
}
```

**iOS**:

```text
{
    "aps": {
        "alert": "$(message)",
        "picture": "$(picture)",
        "view": "$(viewid)"
    }
}
```

**Windows**:

```xml
<toast>
    <visual>
        <binding template="genericTemplate">
            <image id="1" src="$(picture)" />
            <text id="2">$(message)</text>
        </binding>
    </visual>
</toast>
```

!!! tip "Toast, Tile and Badge Schemas"
    If you want to understand the format of the XML that we are using in the Windows section, it's laid out in the [MSDN documentation][1].

Each of these formats can be specified in the appropriate registration call:

```csharp
    // Android Version
    var genericTemplate = new PushTemplate
    {
        Body = "{\"data\":{\"message\":\"$(message)\",\"picture\":\"$(picture)\",\"view\":\"$(viewid)\"}}"
    };
    installation.Templates.Add("genericTemplate", genericTemplate);

    // iOS Version
    var genericTemplate = new PushTemplate
    {
        Body = "{\"aps\":{\"alert\":\"$(message)\",\"picture\":\"$(picture)\",\"view\":\"$(viewid)\"}}"
    };
    installation.Templates.Add("genericTemplate", genericTemplate);

    // Windows Version
    var genericTemplate = new WindowsPushTemplate
    {
        Body = "<toast><visual><binding template=\"genericTemplate\"><image id=\"1\" src=\"$(picture)\"/><text id=\"1\">$(message)</text></binding></visual></toast>"
    };
    genericTemplate.Headers.Add("X-WNS-Type", "wns/toast");
    installation.Templates.Add("genericTemplate", genericTemplate);
```

To push, we can use the same Test Send facility in the Azure Portal.  In the Test Send screen, set the **Platforms** field to be **Custom Template**, and the payload to be a JSON document with the three fields:

```text
{
    "message": "Test Message",
    "picture": "advertisement1",
    "viewid": "advertising"
}
```

If you have done all the changes thus far, you will receive the same notification as before.  The difference is that you are pushing a message once and receiving that same message across all the Android, iOS and Windows systems at the same time.  You no longer have to know what sort of device your users are holding - the message will get to them.

We can take this a step further, however, by deep-linking.  Deep-linking is a technique often used in push notification systems whereby we present the user a dialog that asks them to open the notification.  If the notification is opened, they are taken directly to a new view with the appropriate content provided.

### Deep Linking with Android

Let's start our investigation with the Android code-base.  Our push notification is received by the `OnMessage()` method within the `GcmService` class in the `GcmHandler.cs` file.  We can easily extract the three fields we need to execute our deep-link:

```csharp
    protected override void OnMessage(Context context, Intent intent)
    {
        var message = intent.Extras.GetString("message");
        var picture = intent.Extras.GetString("picture");
        var view = intent.Extras.GetString("view");

        // Rest of code
    }
```

!!! tip "Keep the Push Small"
    You should keep the push payload as small as possible.  There are limits and they vary by platform (but are in the range of 4-5Kb).  Note that I don't include the full URL of the picture, for example, nor do I include the picture as binary data.  This allows me to adjust to an appropriate image URLwithin the client.  This keeps the number of bytes in the push small, but also allows me to adjust the image for the platform, if necessary.

## Push to Sync

Sometimes, you want to alert the user that there is something new for that user.  When the user is alerted, acceptance of the push notification indicates that the user wants to go to the app and synchronize the database before viewing the data.

## Secure Push

Push notifications are insecure.  They appear on the front page of the lock screen and anyone can open them in a multi-user environment (where a mobile device is shared by a community of users).  In these cases, you may want to ensure that the push notification is only opened by the user for which it was intended.

<!-- Links -->
[1]: https://msdn.microsoft.com/en-us/library/windows/apps/br212853.aspx
