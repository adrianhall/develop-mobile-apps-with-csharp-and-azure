## Enterprise Authentication

Enterprise Authentication is handled by Azure Active Directory, which is fairly commonly configured within Azure.  Every Azure subscription has a default directory associated with it that you can leverage for this section.  In addition, if your organization has an Office 365 subscription, this will likely be tied to an Azure Active Directory domain to allow enterprise sign-in.  In either case, you have a directory you can use for providing authentication to your app.

In general, you will need to get special permissions to update the directory. If you want to use your organizations corporate directory, then you are likely to have to get your IT department involved to set it up.

## Azure Active Directory: Server-Flow setup

The Azure Active Directory server-flow is perhaps the easiest of all the authentication methods to configure.  No
matter if you are doing a client flow or server flow, you need to set up the server flow first.

!!! tip
    We recommend that you implement Client Flow in any non-trivial application.

If you are using your default directory and you want to add a couple of test users, you will need to set those up
first.   Start by logging in to the [Azure portal][portal].

1. In the left-hand menu, click **More services**.
2. Enter **Active Directory** in the search box, then click **Azure Active Directory**.
3. (Optional) If you need to manage a different directory to the default directory, click **Switch directory** and choose a different directory.
4. Click **Users and groups**, then **All users**.
    
    ![AzureAD: All Users][img1]

5. Click **Add**.

    ![AzureAD: Add User][img2]

6. Fill in the information.  Ensure you add the user to a group, if necessary.  You can also add additional information (like a real name) in the **Profile** section.
7. Check **Show Password** and make a note of the new password.
8. Click **Create**.

Repeat for each test user you wish to use.  Once done, move onto configuring your App Service for authentication:

1. Click **All resources** in the left hand menu.
2. Click your App Service or Mobile App.
3. Search for and click **Authentication / Authorization** (it's under SETTINGS).
4. Change App Service Authentication to **On**. 
5. Ensure the **Action to take when request is not authenticated** is set to **Allow Anonymous requests**.
6. Click **Azure Active Directory**.
7. Click **Express**.
   
    ![AzureAD: Configuration][img26]

8. All the information is filled in for you.  Click **OK**, then **Save**.

!!! info
    Make sure you create the app service in the right directory / subscription.  If you have access to more than one directory, you can choose the right one by selecting it under your account drop-down in the top-right corner.

There is also an **Advanced** track.  This is used in client-flow situations and in situations where you have more
than one directory.  The Express flow is great for getting started quickly.

!!! info "Preview Portal Access"
    Azure Active Directory portal access is in preview right now.  Certain things can only be done through
    the [Azure Classic Portal][classic-portal].  The list of things that cannot be done in the Azure Portal
    is thankfully dwindling.

You can walk through a server-flow authentication to test that you have all the settings correct.  Point your browser at https://_yoursite_.azurewebsites.net/.auth/login/aad.  The browser will take you through an authentication flow before giving you a successful authentication image:

![AzureAD: Success][img7]

If you get an error akin to "We are having problems signing you in", use an Incognito or InPrivate browsing window.

## Adding Authentication to a Mobile Client

Now that the backend is completely configured, we can move our attention to the mobile client.  We are going to be
using the same mobile client that we developed in the first chapter, but we are now going to add authentication to
it.  Web views are one of those items that are platform dependent. Fortunately for us, Xamarin has already thought
of this and provided a facility for running platform specific code called the [DependencyService][22].

!!! info
    If we run our application right now, clicking on the "Enter the App" button will result in an error.  We will be
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

Finally, we will want to easily instantiate the cloud provider.  In the shared project, add the following to the
constructor for `App.cs`:

```csharp
    public App()
    {
        ServiceLocator.Instance.Add<ICloudService, AzureCloudService>();
        MainPage = new NavigationPage(new Pages.EntryPage());
    }
```

The [ServiceLocator][1] class is used to manage the singletons in an application.  The cloud service object can now be retrieved in any view using the following snippet:

```csharp
    ICloudService cloudService = ServiceLocator.Instance.Resolve<ICloudService>();
```

We will need to do this in the `ViewModels/TaskDetailViewModel.cs` and `ViewModels/TaskKListViewModel.cs` classes.  Refer to the [project in GitHub][10] if you run into issues here.

In each platform-specific project, we need to define a concrete implementation of the login provider that uses a web view to hold the actual authentication flow.  Here is the droid `Services\DroidLoginProvider.cs` (in the
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

Let us take a closer look at this implementation.  The `LoginAsync()` method on the Azure Mobile Apps client object takes the Android context (which is normally the main window) and a provider - we can pick any of "facebook", "google", "microsoftaccount", "twitter" or "aad" depending on what we have defined in the Azure App Service.  The clever piece is the `Xamarin.Forms.Dependency` call at the top - that registers the class as a platform service so we can access it through the Xamarin dependency service.

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

!!! info
    The `ServiceLocator` class is a basic singleton handler.  It is available in the [Chapter2][10] project.  It
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

* Log on to the [Azure portal][portal].
* Select **Azure Active Directory** from the left hand menu.
* Click **App registrations**.

  ![Azure AD Apps][img32]

* Note that our existing web application is already there.  You will see more applications, depending on what you
  have set up.
* Click **+ Add** at the top of the page.

  ![Azure AD Apps - Add an App][img33]

* Enter a name for the app registration.  Select **Native** as the application type.  Enter a valid URI in the Redirect URI.  It can be anything, but it has to be valid.
* Click **Create**.
* In the **Settings** blade, click **Redirect URIs**.
* Add a Redirect URI of the form: `https://yoursite.azurewebsites.net/.auth/login/done`.

  ![Azure AD Apps - Native App Redirect URI Added][img37]

* Click **Save**.
* Click **Required permissions**.
* Click **+ Add**.
* Click **Select an API**.
* Enter the name of your web application in the search box, and press Enter.

    ![Azure AD Apps - Permissions][img3]

* Click the name of your web application, then click **Select**.
* You will be taken to **Select permissions**.  Click **Access _your web application_**

    ![Azure AD Apps - Permissions][img4]

* Click **Select**, then **Done**.

So, what did we just do there?  We created a new Azure AD app for the native application.  We then gave permission
for the native application to access resources that are protected by the web application.  In our Azure App Service, we configured the service so that the Azure AD web application is used to protect our resources.  The net effect is that our native application OR our web application can access the App Service resources that are protected via the `[Authorize]` attribute.

Before continuing, you will need the **Application ID** and the **Redirect URI** for the NATIVE application.  The Application ID for the native app is available in the **Properties** section of the **Settings** blade in the App Registrations blade:

![Azure AD Apps - The Application ID][img5]

 You can enter
these into the `Helpers\Locations.cs` file in the shared project:

```csharp
namespace TaskList.Helpers
{
    public static class Locations
    {
        public static readonly string AppServiceUrl = "https://zumobook-chapter2.azurewebsites.net";

        public static readonly string AadClientId = "0c3309fe-e392-4ca5-8d54-55f69ae1e0f8";

        public static readonly string AadRedirectUri = "https://zumobook-chapter2.azurewebsites.net/.auth/login/done";

        public static readonly string AadAuthority = "https://login.windows.net/photoadrianoutlook.onmicrosoft.com";
    }
}
```

The **AadClientId** and **AadRedirectUri** must match what was configured in Azure AD for the native app.  The
other piece of information we need to add is the Azure AD Authority for the directory.  This is available in the **Domain names** blade within the Azure Active Directory blade.

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
            var zumoPayload = new JObject()
            {
                ["access_token"] = accessToken
            };
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
[img1]: img/aad-ibiza-allusers.PNG
[img2]: img/aad-ibiza-adduser.PNG
[img3]: img/aad-ibiza-appperms1.PNG
[img4]: img/aad-ibiza-appperms2.PNG
[img5]: img/aad-ibiza-appid.PNG
[img7]: img/auth-success.PNG
[img24]: img/ent-dev-1.PNG
[img25]: img/ent-dev-2.PNG
[img26]: img/ent-dev-3.PNG
[img32]: img/aad-ibiza-allapps.PNG
[img33]: img/aad-ibiza-addanapp.PNG
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
[1]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/blob/main/Chapter2/TaskList/TaskList/Helpers/ServiceLocator.cs
[10]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/tree/main/Chapter2
[22]: https://developer.xamarin.com/guides/xamarin-forms/dependency-service/

