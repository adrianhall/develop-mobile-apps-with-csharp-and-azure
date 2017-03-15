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
        "picture": "$(picture)"
    }
}
```

**iOS**:

```text
{
    "aps": {
        "alert": "$(message)",
        "picture": "$(picture)"
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
        Body = "{""data"":{""message"":""$(message)"",""picture"":""$(picture)""}}"
    };
    installation.Templates.Add("genericTemplate", genericTemplate);

    // iOS Version
    var genericTemplate = new PushTemplate
    {
        Body = "{""aps"":{""alert"":""$(message)"",""picture"":""$(picture)""}}"
    };
    installation.Templates.Add("genericTemplate", genericTemplate);

    // Windows Version
    var genericTemplate = new WindowsPushTemplate
    {
        Body = "<toast><visual><binding template=""genericTemplate""><image id=""1"" src=""$(picture)""/><text id=""1"">$(message)</text></binding></visual></toast>"
    };
    genericTemplate.Headers.Add("X-WNS-Type", "wns/toast");
    installation.Templates.Add("genericTemplate", genericTemplate);
```

To push, we can use the same Test Send facility in the Azure Portal.  In the Test Send screen, set the **Platforms** field to be **Custom Template**, and the payload to be a JSON document with the three fields:

```text
{
    "message": "Test Message",
    "picture": "http://r.ddmcdn.com/w_606/s_f/o_1/cx_0/cy_15/cw_606/ch_404/APL/uploads/2014/06/01-kitten-cuteness-1.jpg"
}
```

If you have done all the changes thus far, you will receive the same notification as before.  The difference is that you are pushing a message once and receiving that same message across all the Android, iOS and Windows systems at the same time.  You no longer have to know what sort of device your users are holding - the message will get to them.

We can take this a step further, however, by deep-linking.  Deep-linking is a technique often used in push notification systems whereby we present the user a dialog that asks them to open the notification.  If the notification is opened, they are taken directly to a new view with the appropriate content provided.

### Deep Linking with Android

Let's start our investigation with the Android code-base.  Our push notification is received by the `OnMessage()` method within the `GcmService` class in the `GcmHandler.cs` file.  We can easily extract the two fields we need to execute our deep-link:

```csharp
protected override void OnMessage(Context context, Intent intent)
{
    Log.Info("GcmService", $"Message {intent.ToString()}");
    var message = intent.Extras.GetString("message") ?? "Unknown Message";
    var picture = intent.Extras.GetString("picture");
    CreateNotification("TaskList", message, picture);
}
```

We can continue by implementing a special format of the notification message we used earlier to send a notification:

```csharp
private void CreateNotification(string title, string msg, string parameter = null)
{
    var startupIntent = new Intent(this, typeof(MainActivity));
    startupIntent.PutExtra("param", parameter);

    var stackBuilder = TaskStackBuilder.Create(this);
    stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
    stackBuilder.AddNextIntent(startupIntent);

    var pendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.OneShot);
    var notification = new Notification.Builder(this)
        .SetContentIntent(pendingIntent)
        .SetContentTitle(title)
        .SetContentText(msg)
        .SetSmallIcon(Resource.Drawable.icon)
        .SetAutoCancel(true)
        .Build();
    var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
    notificationManager.Notify(0, notification);
}
```

The additional piece is the `startupIntent`.  When the user clicks on open, the mobile app is called with the `startupIntent` included in the context.  We update the `OnCreate()` method with `MainActivity.cs` to read this intent:

```csharp
[Activity(Label = "TaskList.Droid", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
{
    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

        global::Xamarin.Forms.Forms.Init(this, bundle);

        ((DroidPlatformProvider)DependencyService.Get<IPlatformProvider>()).Init(this);

        string param = this.Intent.GetStringExtra("param");
        LoadApplication(new App(loadParameter: param));
    }
}
```

The param string is null on the first start (or when the intent is not present).  This get's passed to our `App()` constructor (in the shared project):

```csharp
public App(string loadParameter = null)
{
    ServiceLocator.Instance.Add<ICloudService, AzureCloudService>();

    if (loadParameter == null)
    {
        MainPage = new NavigationPage(new Pages.EntryPage());
    }
    else
    {
        MainPage = new NavigationPage(new Pages.PictureView(loadParameter));
    }
}
```

If the `App()` constructor is passed a non-null parameter, then we deep-link to a new page instead of going to the entry page.  Now all we need to do is create a XAML page as follows that loads a picture.  The `Pages.PictureView.xaml` is small enough since its only function is to display a picture:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="TaskList.Pages.PictureView"
    xmlns="http://xamarin.com/schemas/2014/forms"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    <Image x:Name="background" Source="{Binding PictureSource, Mode=OneWay}" />
</ContentPage>
```

The code behind file looks similar to the `TaskDetail` page:

```csharp
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TaskList.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PictureView : ContentPage
    {
        public PictureView(string picture)
        {
            InitializeComponent();
            BindingContext = new ViewModels.PictureViewModel(picture);
        }
    }
}
```

Finally, the view model should be familiar at this point:

```csharp
using TaskList.Abstractions;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class PictureViewModel : BaseViewModel
    {
        public PictureViewModel(string picture = null)
        {
            if (picture != null)
            {
                PictureSource = picture;
            }
            Title = "A Picture for you";
        }

        public string PictureSource { get; }
    }
}
```

If I were to continue, I would add some controls that allow me to go back to the task list (if I am logged in) or the entry page (if I am not logged in).

!!! tip "Keep the Push Small"
    You should keep the push payload as small as possible.  There are limits and they vary by platform (but are in the range of 4-5Kb).  Note that I don't include the full URL of the picture, for example, nor do I include the picture as binary data.  This allows me to adjust to an appropriate image URLwithin the client.  This keeps the number of bytes in the push small, but also allows me to adjust the image for the platform, if necessary.

### Deep Linking with iOS


## Push to Sync

Sometimes, you want to alert the user that there is something new for that user.  When the user is alerted, acceptance of the push notification indicates that the user wants to go to the app and synchronize the database before viewing the data.

## Secure Push

Push notifications are insecure.  They appear on the front page of the lock screen and anyone can open them in a multi-user environment (where a mobile device is shared by a community of users).  In these cases, you may want to ensure that the push notification is only opened by the user for which it was intended.

<!-- Links -->
[1]: https://msdn.microsoft.com/en-us/library/windows/apps/br212853.aspx
