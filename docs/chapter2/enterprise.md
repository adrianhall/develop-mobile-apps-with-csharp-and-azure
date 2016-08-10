## Enterprise Authentication

Enterprise Authentication is handled by Azure Active Directory, which is fairly commonly configured within Azure.
Every Azure subscription has a default directory associated with it that you can leverage for this section.  In
addition, if your organization has an Office 365 subscription, this will likely be tied to an Azure Active Directory
domain to allow enterprise sign-in.  In either case, you have a directory you can use for providing authentication
to your app.

In general, you will need to get special permissions to update the directory. If you want to use your organizations
corporate directory, then you are likely to have to get your IT department involved to set it up.

## Azure Active Directory: Server-Flow setup

The Azure Active Directory server-flow is perhaps the easiest of all the authentication methods to configure.  No
matter if you are doing a client flow or server flow, you need to set up the server flow first.

> We recommend that you implement Client Flow in any non-trivial application.

If you are using your default directory and you want to add a couple of test users, you will need to set those up
first.   Start by going to the [Classic Azure Portal][classic-portal]:

![Classic Portal][img24]

Click on the **Default Directory**, then click on **USERS**.  You will notice that your Azure-linked ID is already
present.

![AzureAD: Users][img25]

Click on **Add User** at the bottom of the screen.  Enter a username in the box provided before clicking on the
arrow.  Then fill in the personal information and click on the arrow again.  Finally, click on **create**.  Note
the password, before clicking on the tick.  Now you have two users - your Azure ID and the one you just created.
Note the username.  It is based on the tenant, so it will be something like `adrian@photoadrianoutlook.onmicrosoft.com`.

To configure your app, switch back to the regular [Azure Portal][portal], find your App Service, click on
**All Settings** followed by **Authentication / Authorization**. Finally, select **Azure Active Directory**.

![AzureAD: Configuration][img26]

Click on **Express**.  Note that all the information is filled in for you.  All you have to do is click on **OK**,
followed by **Save**.

> Make sure you create the app service in the right directory / subscription.  If you have access to more than one
 directory, you can choose the right one by selecting it under your account drop-down in the top-right corner.

There is also an **Advanced** track.  This is used in client-flow situations and in situations where you have more
than one directory.  The Express flow is great for getting started quickly.

You can walk through a server-flow authentication to test that you have all the settings correct.  Point your browser
at https://_yoursite_.azurewebsites.net/.auth/login/aad.  The browser will take you through an authentication flow
before giving you a successful authentication image:

![AzureAD: Success][img7]

## Adding Authentication to a Mobile Client

Now that the backend is completely configured, we can move our attention to the mobile client.  We are going to be
using the same mobile client that we developed in the first chapter, but we are now going to add authentication to
it.  Web views are one of those items that are platform dependent. Fortunately for us, Xamarin has already thought
of this and provided a facility for running platform specific code called the [DependencyService][22].

> If we run our application right now, clicking on the "Enter the App" button will result in an error.  You will be
able to see the Unauthorized error in the debug window of Visual Studio.

Our first step is to define an `Abstractions\ILoginProvider.cs` interface within the  shared project:

```csharp
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace TaskList.Abstractions
{
    public interface ILoginProvider
    {
        Task LoginAsync(MobileServiceClient client);
    }
}
```

Next, we are going to extend our `Abstractions\ICloudService.cs` interface so that the main application can call
the login routine:

```csharp
using System.Threading.Tasks;

namespace TaskList.Abstractions
{
    public interface ICloudService
    {
        ICloudTable<T> GetTable<T>() where T : TableData;

        Task LoginAsync();
    }
}
```

Our code will call `LoginAsync()` in the `ICloudService`, which will get the platform-specific version of the
login provider and call `LoginAsync()` there, but with our defined mobile service client.  That is defined in the
`Services\AzureCloudService.cs` class:

```csharp
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;
using TaskList.Helpers;
using Xamarin.Forms;

namespace TaskList.Services
{
    public class AzureCloudService : ICloudService
    {
        MobileServiceClient client;

        public AzureCloudService()
        {
            client = new MobileServiceClient(Locations.AppServiceUrl);
        }

        public ICloudTable<T> GetTable<T>() where T : TableData => new AzureCloudTable<T>(client);

        public Task LoginAsync()
        {
            var loginProvider = DependencyService.Get<ILoginProvider>();
            return loginProvider.LoginAsync(client);
        }
    }
}
```

The method looks up the platform dependent version of the login provider and executes the login method, passing
along the client (which we will need later).

In each platform-specific project, we are going to define a concrete implementation of the login provider that uses
a web view to hold the actual authentication flow.  Here is the droid `Services\DroidLoginProvider.cs` (in the
TaskList.Droid project):

```csharp
using System.Threading.Tasks;
using Android.Content;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;
using TaskList.Droid.Services;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace TaskList.Droid.Services
{
    public class DroidLoginProvider : ILoginProvider
    {
        Context context;

        public void Init(Context context)
        {
            this.context = context;
        }

        public async Task LoginAsync(MobileServiceClient client)
        {
            await client.LoginAsync(context, "aad");
        }
    }
}
```

Let us take a closer look at this implementation.  The `LoginAsync()` method on the Azure Mobile Apps client object
takes the Android context (which is normally the main window) and a provider - we can pick any of "facebook",
"google", "microsoftaccount", "twitter" or "aad" depending on what we have defined in the Azure App Service.  The
clever piece is the `Xamarin.Forms.Dependency` call at the top - that registers the class as a platform service
so we can access it through the Xamarin dependency service.

Note that we need an extra initialization routine for Android that must be called prior the login provider being
called to pass along the main window of the app (also known as the context).  This is done in the `MainActivity.cs`
file **after** the Xamarin Forms initialization call.  The dependency service is not set up until after the Xamarin
Forms library is initialized, so we will not be able to get the login provider reference before that point:

```csharp
protected override void OnCreate(Bundle bundle)
{
    base.OnCreate(bundle);

    Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

    global::Xamarin.Forms.Forms.Init(this, bundle);

    ((DroidLoginProvider)DependencyService.Get<ILoginProvider>()).Init(this);

    LoadApplication(new App());
}
```

iOS is similar, but does not require the initialization step in the main startup class.  The login provider class
is in `Services\iOSLoginProvider.cs` (in the **TaskList.iOS** project):

```csharp
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;
using TaskList.iOS.Services;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(iOSLoginProvider))]
namespace TaskList.iOS.Services
{
    public class iOSLoginProvider : ILoginProvider
    {
        public async Task LoginAsync(MobileServiceClient client)
        {
            await client.LoginAsync(RootView, "aad");
        }

        public UIViewController RootView => UIApplication.SharedApplication.KeyWindow.RootViewController;
    }
}
```

Note that we are using the same pattern here for registering the concrete implementation with the dependency service,
so we can get it the same way. Finally, here is the UWP `Services\UWPLoginProvider.cs` (in the TaskList.UWP project):

```csharp
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;
using TaskList.UWP.Services;

[assembly: Xamarin.Forms.Dependency(typeof(UWPLoginProvider))]
namespace TaskList.UWP.Services
{
    public class UWPLoginProvider : ILoginProvider
    {
        public async Task LoginAsync(MobileServiceClient client)
        {
            await client.LoginAsync("aad");
        }
    }
}
```

Now that we have all the platform-specific login routines registered, we can move on to adding the login routine to
the UI.  We have already got a button on the entry page to enter the app.  It makes sense to wire up that button so
that it logs us in as well. The Command for the login button is in the `ViewModels\EntryPageViewModel.cs`:

```csharp
async Task ExecuteLoginCommand()
{
    if (IsBusy)
        return;
    IsBusy = true;

    try
    {
        var cloudService = ServiceLocator.Instance.Resolve<ICloudService>();
        await cloudService.LoginAsync();
        Application.Current.MainPage = new NavigationPage(new Pages.TaskList());
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"[ExecuteLoginCommand] Error = {ex.Message}");
    }
    finally
    {
        IsBusy = false;
    }
}
```

> The `ServiceLocator` class is my basic singleton handler.  It is available in the [Chapter2][10] project.  It
returns the concrete version of the cloud service, just like the Singleton version we defined in Chapter1.

When you run the application, clicking on the "Enter the App" button will now present you with an Authenticate window:

![AAD Authenticate][img58]

Going through the authentication process will get you to the task list again.  If the authentication process fails,
then `LoginAsync()` will throw an error, which is caught at the ViewModel.  Right now, the `EntryPageViewModel`
does nothing more than print a diagnostic message to the debug window of Visual Studio.

## Azure Active Directory: Client-Flow Setup

Configuring Azure Active Directory for client-flow is a three-step process.  First, we need to create a WEB
application.  This represents the resource: in our case, the resource is the Azure Mobile Apps backend.  Then
we need to create a NATIVE application.  This represents the client: in our case, the ADAL (Active Directory
Access Library) library will need this information.  Finally, we need to give the NATIVE application permission
to access the WEB application.

It starts with configuring a server-flow to protect the resource.  We've already done that above. Then configure
a "Native Application" and give it permissions to the web application:

* Log on to the [Classic Portal][classic-portal].
* Select the **Default Directory** from your list of all items.
* Click on the **APPLICATIONS** tab.

  ![Azure AD Apps][img32]

* Note that our existing web application is already there.  You will see more applications, depending on what you
  have set up.  In this example, I have Visual Studio Team Services and Office 365 set up.
* Click on the **ADD** button at the bottom of the page.

  ![Azure AD Apps - Add an App][img33]

* Click on **Add an application my organization is developing**.

  ![Azure AD Apps - Add a Native App][img34]

* Enter a name for the app registration, and select **NATIVE CLIENT APPLICATION**.
* Click on the Next arrow.
* Enter a valid URI - it can be anything, but it has to be valid

  ![Azure AD Apps - Native App Redirect URI][img35]

* Click on the tick to create the application.
* The wizard will close, but you will be brought to the app configuration.  Click on the **CONFIGURE** tab.

  ![Azure AD Apps - Native App Configuration][img36]

* Add a Redirect URI of the form: `https://yoursite.azurewebsites.net/.auth/login/done`.

  ![Azure AD Apps - Native App Redirect URI Added][img37]

* At the bottom of the page is the **permissions to other applications** section.  Click on the **Add application** button.

  ![Azure AD Apps - Permissions (1)][img38]

* Click on the **SHOW** drop-down and select **All Apps**, then click on the tick next to the search box.
* Click on the web application that you set up during the server-flow configuration, then click on the  tick in
  the lower-right corner.

  ![Azure AD Apps - Permissions (2)][img39]

* Click on **Delegated Permissions** next to the web application.  Check the box next to **Access**, then click
  on **Save** at the bottom of the screen.

  ![Azure AD Apps - Permissions (3)][img40]

At this point the application configuration will be saved.

So, what did we just do there?  We created a new Azure AD app for the native application.  We then gave permission
for the native application to access resources that are protected by the web application.  In our Azure App Service,
we configured the service so that the Azure AD web application is used to protect our resources.  The net effect is
 that our native application OR our web application can access the App Service resources that are protected via the
 `[Authorize]` attribute.

Before continuing, you will need the **Client ID** and the **Redirect URI** for the NATIVE application. You can enter
these into the `Helpers\Locations.cs` file in the shared project:

```csharp
namespace TaskList.Helpers
{
    public static class Locations
    {
        public static readonly string AppServiceUrl = "https://the-book.azurewebsites.net";

        public static readonly string AadClientId = "b61c7d68-2086-43a1-a8c9-d93c5732cc84";

        public static readonly string AadRedirectUri = "https://the-book.azurewebsites.net/.auth/login/done";

        public static readonly string AadAuthority = "https://login.windows.net/photoadrianoutlook.onmicrosoft.com";
    }
}
```

The **AadClientId** and **AadRedirectUri** must match what we have configured in Azure AD for the native app.  The
other piece of information we need to add is the Azure AD Authority for the directory.  If you click on the
**DOMAINS** tab, it will generally tell you what domain you are in. The Authority is just a path on the
`https://login.windows.net` that corresponds to your domain.  There is also a GUID version of this domain.  You
can find the GUID by looking at the **View Endpoints** in the **APPLICATIONS** tab.  Look at the first path section
of most all the endpoints.

Add the **Microsoft.IdentityModel.Clients.ActiveDirectory** NuGet package using **Manage NuGet Packages...** to
each platform project.  This package contains the ADAL library as a portable class library.

![Azure AD - Add ADAL Library][img41]

Now you can add the client flow to each project.  Start with the login provider in the **TaskList.UWP** project,
located in the `Services\UWPLoginProvider.cs` file:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.UWP.Services;

[assembly: Xamarin.Forms.Dependency(typeof(UWPLoginProvider))]
namespace TaskList.UWP.Services
{
    public class UWPLoginProvider : ILoginProvider
    {

        /// <summary>
        /// Login via ADAL
        /// </summary>
        /// <returns>(async) token from the ADAL process</returns>
        public async Task<string> LoginADALAsync()
        {
            Uri returnUri = new Uri(Locations.AadRedirectUri);

            var authContext = new AuthenticationContext(Locations.AadAuthority);
            if (authContext.TokenCache.ReadItems().Count() > 0)
            {
                authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
            }
            var authResult = await authContext.AcquireTokenAsync(
                Locations.AppServiceUrl, /* The resource we want to access  */
                Locations.AadClientId,   /* The Client ID of the Native App */
                returnUri,               /* The Return URI we configured    */
                new PlatformParameters(PromptBehavior.Auto, false));
            return authResult.AccessToken;
        }


        public async Task LoginAsync(MobileServiceClient client)
        {
            // Client Flow
            var accessToken = await LoginADALAsync();
            var zumoPayload = new JObject();
            zumoPayload["access_token"] = accessToken;
            await client.LoginAsync("aad", zumoPayload);

            // Server-Flow Version
            // await client.LoginAsync("aad");
        }
    }
}
```

The `LoginADALAsync()` method does the actual client-flow - using the ADAL library to authenticate the user and
return the access token.  The `LoginAsync()` method initiates the client-flow.  It uses the token it receives
from the client-flow to log in to the App Service, by packaging the token into a JSON object.  I have placed
the client and server flow next to each other so you can compare the two.

In the **TaskList.Droid** project, we need to deal with the `Context`, as is common with Android libraries.  The
client flow in `Services\DroidLoginProvider.cs` is remarkably similar though:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Droid.Services;
using TaskList.Helpers;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace TaskList.Droid.Services
{
    public class DroidLoginProvider : ILoginProvider
    {
        Context context;

        public void Init(Context context)
        {
            this.context = context;
        }

        /// <summary>
        /// Login via ADAL
        /// </summary>
        /// <returns>(async) token from the ADAL process</returns>
        public async Task<string> LoginADALAsync()
        {
            Uri returnUri = new Uri(Locations.AadRedirectUri);

            var authContext = new AuthenticationContext(Locations.AadAuthority);
            if (authContext.TokenCache.ReadItems().Count() > 0)
            {
                authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
            }
            var authResult = await authContext.AcquireTokenAsync(
                Locations.AppServiceUrl, /* The resource we want to access  */
                Locations.AadClientId,   /* The Client ID of the Native App */
                returnUri,               /* The Return URI we configured    */
                new PlatformParameters((Activity)context));
            return authResult.AccessToken;
        }

        public async Task LoginAsync(MobileServiceClient client)
        {
            // Client Flow
            var accessToken = await LoginADALAsync();
            var zumoPayload = new JObject();
            zumoPayload["access_token"] = accessToken;
            await client.LoginAsync("aad", zumoPayload);

            // Server-Flow Version
            // await client.LoginAsync(context, "aad");
        }
    }
}
```

The only real difference between this one and the Universal Windows edition is the PlatformParameters. We need to
pass in the context of the MainActivity (which is passed in through the `Init()` call).  However, we must also handle
the response from the ADAL library.  This is done in `MainActivity.cs`. Add the following method to the `MainActivity`
class:

```csharp
protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
{
    base.OnActivityResult(requestCode, resultCode, data);
    AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
}
```

Finally, the iOS version also requires access to the root view, so its `PlatformParameters` are also slightly
different.  Here is `Services\iOSLoginProvider.cs`:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.iOS.Services;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(iOSLoginProvider))]
namespace TaskList.iOS.Services
{
    public class iOSLoginProvider : ILoginProvider
    {
        /// <summary>
        /// Login via ADAL
        /// </summary>
        /// <returns>(async) token from the ADAL process</returns>
        public async Task<string> LoginADALAsync(UIViewController view)
        {
            Uri returnUri = new Uri(Locations.AadRedirectUri);

            var authContext = new AuthenticationContext(Locations.AadAuthority);
            if (authContext.TokenCache.ReadItems().Count() > 0)
            {
                authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
            }
            var authResult = await authContext.AcquireTokenAsync(
                Locations.AppServiceUrl, /* The resource we want to access  */
                Locations.AadClientId,   /* The Client ID of the Native App */
                returnUri,               /* The Return URI we configured    */
                new PlatformParameters(view));
            return authResult.AccessToken;
        }

        public async Task LoginAsync(MobileServiceClient client)
        {
            var rootView = UIApplication.SharedApplication.KeyWindow.RootViewController;

            // Client Flow
            var accessToken = await LoginADALAsync(rootView);
            var zumoPayload = new JObject();
            zumoPayload["access_token"] = accessToken;
            await client.LoginAsync("aad", zumoPayload);

            // Server Flow
            //await client.LoginAsync(rootView, "aad");
        }
    }
}
```

Note that we can balance the needs of each platform by using the dependency service.  The code that is unique to
the platform is minimized and stored with the platform.

If you aren't interested in social authentication (Facebook, Google, Microsoft or Twitter authentication providers),
you can [skip the Social Authentication section](debugging.md).

<!-- Images -->
[img7]: img/auth-success.PNG
[img24]: img/ent-dev-1.PNG
[img25]: img/ent-dev-2.PNG
[img26]: img/ent-dev-3.PNG
[img32]: img/aad-apps-1.PNG
[img33]: img/aad-apps-2.PNG
[img34]: img/aad-apps-3.PNG
[img35]: img/aad-apps-4.PNG
[img36]: img/aad-apps-5.PNG
[img37]: img/aad-apps-6.PNG
[img38]: img/aad-apps-7.PNG
[img39]: img/aad-apps-8.PNG
[img40]: img/aad-apps-9.PNG
[img41]: img/adal-client-1.PNG
[img58]: img/aad-logon-window.PNG

<!-- External Links -->
[classic-portal]: https://manage.windowsazure.com/
[portal]: https://portal.azure.com/
[10]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/tree/master/Chapter2
[22]: https://developer.xamarin.com/guides/xamarin-forms/dependency-service/