This section is dedicated to exploring various common recipes for push notifications and how we can achieve those recipes through cross-platform code.

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
<?xml version="1.0" encoding="utf-8"?>
<toast launch="zumobook">
  <visual>
    <binding template="ToastGeneric">
      <text>$(message)</text>
    </binding>
  </visual>
  <actions>
    <action content="Open" arguments="$(picture)" />
    <action content="Cancel" arguments="cancel" />
  </actions>
</toast>
```

!!! tip "Toast, Tile and Badge Schemas"
    If you want to understand the format of the XML that we are using in the Windows section, it's laid out in the [MSDN documentation][1].

Each of these formats can be specified in the appropriate registration call:

```csharp
    // Android Version
    var genericTemplate = new PushTemplate
    {
        Body = @"{""data"":{""message"":""$(message)"",""picture"":""$(picture)""}}"
    };
    installation.Templates.Add("genericTemplate", genericTemplate);

    // iOS Version
    var genericTemplate = new PushTemplate
    {
        Body = @"{""aps"":{""alert"":""$(message)"",""picture"":""$(picture)""}}"
    };
    installation.Templates.Add("genericTemplate", genericTemplate);

    // Windows Version
    var genericTemplate = new WindowsPushTemplate
    {
        Body = @"<?xml version=""1.0"" encoding=""utf-8""?>
<toast launch=""zumobook"">
  <visual>
    <binding template=""ToastGeneric"">
      <text>$(message)</text>
    </binding>
  </visual>
  <actions>
    <action content=""Open"" arguments=""$(picture)"" />
    <action content=""Cancel"" arguments=""cancel"" />
  </actions>
</toast>"
    };
    genericTemplate.Headers.Add("X-WNS-Type", "wns/toast");
    installation.Templates.Add("genericTemplate", genericTemplate);
```

To push, we can use the same Test Send facility in the Azure Portal.  In the Test Send screen, set the **Platforms** field to be **Custom Template**, and the payload to be a JSON document with the two fields:

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

Deep linking with iOS follows a similar pattern to Android.  The notification is received by `DidReceiveRemoteNotification()` method in the `AppDelegate.cs`, which we can then process to load the appropriate page from the background.

First, update the `DidReceiveRemoteNotification()` method to call a new method we will define in a moment.  This allows us to call the notification processor from multiple places:

```csharp
/// <summary>
/// Handler for Push Notifications
/// </summary>
public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
{
    ProcessNotification(userInfo, false);
}
```

This method is also defined in the `AppDelegate.cs` class:

```csharp
private void ProcessNotification(NSDictionary options, bool fromFinishedLoading)
{
    if (!(options != null && options.ContainsKey(new NSString("aps"))))
    {
        // Short circuit - nothing to do
        return;
    }

    NSDictionary aps = options.ObjectForKey(new NSString("aps")) as NSDictionary;

    // Obtain the alert and picture elements if they are there
    var alertString = GetStringFromOptions(aps, "alert");
    var pictureString = GetStringFromOptions(aps, "picture");

    if (!fromFinishedLoading)
    {
        // Manually show an alert
        if (!string.IsNullOrEmpty(alertString))
        {
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
```

This method checks to see if there is something to do.  If there is, it generates the alert as before.  This time, however, if the user clicks on OK, then it sets the current page to the same `PictureView` view that was used by the Android application.  The `GetStringFromOptions()` method is a convenience method for extracting strings from the push notification payload.

Send the following push notification to receive the picture:

```text
{
    "aps":{
        "alert":"Notification Hub test notification",
        "picture":"http://r.ddmcdn.com/w_606/s_f/o_1/cx_0/cy_15/cw_606/ch_404/APL/uploads/2014/06/01-kitten-cuteness-1.jpg"
    }
}
```

You should test this in the following cases:

* The app is running and in the foreground.
* The app is running, but in the background.
* The app is not running at all.

### Deep Linking with UWP

Universal Windows is perhaps the most complete story for notifications out there.  Firstly, let's construct our notification.  On the **Test Send** blade within your notification hub in the Azure portal, choose **Windows** as the platform and cut and paste the following into the Payload:

```xml
<?xml version="1.0" encoding="utf-8"?>
<toast launch="zumobook">
  <visual>
    <binding template="ToastGeneric">
      <text>This is a simple toast notification example</text>
    </binding>
  </visual>
  <actions>
    <action content="Open" arguments="http://static.boredpanda.com/blog/wp-content/uploads/2016/08/cute-kittens-7-57b30aa10707a__605.jpg" />
    <action content="Cancel" arguments="cancel" />
  </actions>
</toast>
```

This payload provides a textual response with two buttons - an open button and a cancel button.  The most important part of this, however, is the `launch="zumobook"`.  If the user clicks on Open, the application it is associated with is launched via the `OnActivated()` method, and the toast information is passed into that method.  This method is located in the `App.xaml.cs` file of the TaskList.UWP project:

```csharp
protected override void OnActivated(IActivatedEventArgs args)
{
    if (args.Kind == ActivationKind.ToastNotification)
    {
        var toastArgs = args as ToastNotificationActivatedEventArgs;
        Xamarin.Forms.Application.Current.MainPage = new Xamarin.Forms.NavigationPage(
            new Pages.PictureView(toastArgs.Argument));
    }
}
```

The only real problem here is that there is a conflict within this file between the standard `Frame` object and the Xamarin Forms version of the `Frame` object.  If you use `using Xamarin.Forms;` in this file, you have to fully qualify conflicting classes.  It's just as easy to fully-qualify the specific Xamarin Forms classes when they are needed, as I did above.

## Push to Sync

Sometimes, you want to alert the user that there is something new for that user.  When the user is alerted, acceptance of the push notification indicates that the user wants to go to the app and synchronize the database before viewing the data.

Push to Sync is very similar to the Marketing Push, but there are some caveats.  In general, the synchronization process should happen within 30 seconds.  That's not very long in the mobile world.  So, what do you do?

Firstly, let's look at the code for the server-side.  We need to generate an asynchronous push whenever a record is updated.  We will pass the ID of the updated record with the push.  Here is an example table controller:

```csharp
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using Backend.DataObjects;
using Backend.Models;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using System.Collections.Generic;
using System;

namespace Backend.Controllers
{
    [Authorize]
    public class TodoItemController : TableController<TodoItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request, enableSoftDelete: true);
        }

        // GET tables/TodoItem
        public IQueryable<TodoItem> GetAllTodoItems()
        {
            return Query();
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TodoItem> GetTodoItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch)
        {
            var item = await UpdateAsync(id, patch);
            await PushToSyncAsync("todoitem", item.Id);
            return item;
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            TodoItem current = await InsertAsync(item);
            await PushToSyncAsync("todoitem", item.Id);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task DeleteTodoItem(string id)
        {
            await PushToSyncAsync("todoitem", id);
            await DeleteAsync(id);
        }

        private async Task PushToSyncAsync(string table, string id)
        {
            var appSettings = this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();
            var nhName = appSettings.NotificationHubName;
            var nhConnection = appSettings.Connections[MobileAppSettingsKeys.NotificationHubConnectionString].ConnectionString;

            // Create a new Notification Hub client
            var hub = NotificationHubClient.CreateClientFromConnectionString(nhConnection, nhName);

            // Create a template message
            var templateParams = new Dictionary<string, string>();
            templateParams["op"] = "sync";
            templateParams["table"] = table;
            templateParams["id"] = id;

            // Send the template message
            try
            {
                var result = await hub.SendTemplateNotificationAsync(templateParams);
                Configuration.Services.GetTraceWriter().Info(result.State.ToString());
            }
            catch (Exception ex)
            {
                Configuration.Services.GetTraceWriter().Error(ex.Message, null, "PushToSync Error");
            }
        }
    }
}
```

The important code here is the `PushToSyncAsync()` method.  This does the actual push to your clients.  In this version, any client that has registered a template with the `$(op)`, `$(table)` and `$(id)` variables will get the push notification.  The Notifications Hub SDK `SendTemplateNotificationAsync()` method can also send to a list of devices and a tag expression via various overloads.

We have to also register a new template.  Here are the two versions:

```csharp
// Android version
var pushToSyncTemplate = new PushTemplate
{
    Body = @"{""data"":{""op"":""$(op)"",""table"":""$(table)"",""id"":""$(id)""}}"
};
installation.Templates.Add("pushToSync", pushToSyncTemplate);

// iOS version
PushTemplate pushToSyncTemplate = new PushTemplate
{
    Body = @"{""aps"":{""op"":""$(op)"",""table"":""$(table)"",""id"":""$(id)""},""content-available"":1}"
}
installation.Templates.Add("pushToSync", pushToSyncTemplate);
```

!!! info "What about Universal Windows"
    You can do some remarkable things with Universal Windows, but you have to resort to raw pushes.  At that 
    point, you can decide what to put in the payload.  When running, these are handled the same way as the 
    marketing push.  For more information, see [the WNS documentation][2].

The Push to Sync message needs to be handled by the TaskList view.  The easiest mechanism of communicating with the view is to use the `MessagingCenter`.  The TaskList view already has the appropriate code to refresh the list when it receives a message:

```csharp
    // Execute the refresh command
    RefreshCommand.Execute(null);
    MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
    {
        await ExecuteRefreshCommand();
    });
```

We can add an appropriate push-to-sync version like this:

```csharp
    MessageCenter.Subscribe<PushToSync>(this, "ItemsChanged", async (sender) => 
    {
        await ExecuteRefreshCommand();
    });
```

This is the same code, but listens for a notification from a different class.  That class is defined in the shared project `Models` folder:

```csharp
namespace TaskList.Models
{
    public class PushToSync
    {
        public string Table { get; set; }
        public string Id { get; set; }
    }
}
```

When the MessagingCenter is sent a message for push-to-sync, it will execute the refresh command, thus refreshing the data.  All that remains is to actually send that message in response to the notification.  For Android, this is done in the `Services\GcmHandler.cs` file in the `OnMessage()` method:

```csharp
    protected override void OnMessage(Context context, Intent intent)
    {
        Log.Info("GcmService", $"Message {intent.ToString()}");
        var op = intent.Extras.GetString("op");
        if (op != null)
        {
            var syncMessage = new PushToSync()
            {
                Table = intent.Extras.GetString("table"),
                Id = intent.Extras.GetString("id")
            };
            MessagingCenter.Send<PushToSync>(syncMessage, "ItemsChanged");
        }
        else
        {
            var message = intent.Extras.GetString("message") ?? "Unknown Message";
            var picture = intent.Extras.GetString("picture");
            CreateNotification("TaskList", message, picture);
        }
    }
```

For iOS, the send happens in the `ProcessNotification()` method of the `AppDelegate.cs` class:

```csharp
    private void ProcessNotification(NSDictionary options, bool fromFinishedLoading)
    {
        if (!(options != null && options.ContainsKey(new NSString("aps"))))
            return;

        NSDictionary aps = options.ObjectForKey(new NSString("aps")) as NSDictionary;
        if (!fromFinishedLoading)
        {
            var alertString = GetStringFromOptions(aps, "alert");
            if (!string.IsNullOrEmpty(alertString))
            {
                // Create the alert (removed for brevity)
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
```

When a client inserts or updates a record into the database on the server, `PushToSync()` is called.  That emits a push notification in the proper form defined within the mobile app.  When the mobile app receives that push notification, it sends an "ItemsChanged" event to the messaging center.  The TaskList view subscribes to those events and performs a sync in response to that event.

There are several things we could do to this code, including:

*  Push to the UserId that owns the record only - this will reduce the number of pushes that happen.
*  Only pull the specific record on the specific table that is needed.  This is available in the sender object.

<!-- Links -->
[1]: https://msdn.microsoft.com/en-us/library/windows/apps/br212853.aspx
[2]: https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-raw-notification-overview
