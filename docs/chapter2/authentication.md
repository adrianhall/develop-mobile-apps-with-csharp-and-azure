# Authentication

One of the very first things you will want to do is to provide users with a
unique experience.  For our example task list application, this could be as
simple as providing a task list for the user who is logged in.  In more complex
applications, this is the gateway to role-based access controls, group rules,
and sharing with your friends.  In all these cases, properly identifying the
user using the phone is the starting point.

Authentication provides a process by which the user that is using the mobile
device can be identified securely.  This is generally done by entering a
username and password.  However, modern systems can also provide
[multi-factor authentication][1], send you a text message to a registered
device, or [use your fingerprint][2] as the password.

## The OAuth Process

In just about every single mobile application, a process called [OAuth][3] is
used to properly identify a user to the mobile backend.  OAuth is not an
authentication mechanism in its own right.  It is used to route the
authentication request to the right place and to verify that the authentication
took place. There are three actors in the OAuth protocol:

* The **Client** is the application attempting to get access to the resource.
* The **Resource** is the mobile backend that the client is attempting to access.
* The **Identity Provider** (or IdP) is the service that is responsible for authenticating the client.

At the end of the process, a cryptographically signed token is minted.  This
token is added to every single subsequent request to identify the user.

## Server Side vs. Client Side Authentication

There are two types of authentication flow: Server-Flow and Client-Flow.  They
are so named because of who controls the flow of the actual authentication.

![Authentication Flow][img1]

Server-flow is named because the authentication flow is managed by the server
through a web connection.  It is generally used in two cases:

* You want a simple placeholder for authentication in your mobile app while you are developing other code.
* You are developing a web app.

In the case of Server Flow:

1. The client brings up a web view and asks for the login page from the resource.
2. The resource redirects the client to the identity provider.
3. The identity provider does the authentication before redirecting the client
   back to the resource (with an identity provider token).
4. The resource validates the identity provider token with the identity provider.
5. Finally, the resource mints a new resource token that it returns to the client.

Client-flow authentication uses an IdP provided SDK to integrate a more native
feel to the authentication flow.  The actual flow happens on the client,
communicating only with the IdP.

1. The client uses the IdP SDK to communicate with the identity provider.
2. The identity provider does the authentication, returning an identity provider token.
3. The client presents the identity provider token to the resource.
4. The resource validates the identity provider token with the identity provider.
5. Finally, the resource mints a new resource token that it returns to the client.

For example, if you use the Facebook SDK for authentication, your app will seamlessly
switch over into the Facebook app and ask you to authorize your client application
before switching you back to your client application.

It is generally recommended that you use the IdP SDK when developing an app
that will be released on the app store.  This follows the best practice provided
by the majority of identity providers and provides the best experience for your
end users.

## Authentication Providers

Azure Mobile Apps supports five identity providers natively:

* Azure Active Directory
* Facebook
* Google
* Microsoft (MSA)
* Twitter

In addition, you can set up client-flow custom authentication that allows
you to mint a ZUMO token to your specifications for any provider using a
client-flow.  For example, you could use authentication providers like
[Azure AD B2C][7], [LinkedIn][4] or [GitHub][5], a third-party authentication
provider like  [Auth0][6], or you could set up an identity table in your
database so that you can check  username and password without an identity
provider.

## Adding Authentication to a Mobile Backend

There is no addition configuration if you wish to use authentication and authorization
with Azure App Service.  There is an additional configuration code block to configure
the backend for local debugging - we will get onto that later.

Authentication is done by the App Service.  Authorization (which is the determination
of whether an authenticated user can use a specific API) happens at one of two levels.
We can add authorization to an entire table controller by adding the `[Authorize]` attribute
to the table controller.  We can also add authorization on individual operations by adding
the `[Authorize]` attribute to individual methods within the table controller. For example,
here is our table controller from the first chapter with authorization required for all
operations:

```csharp
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Backend.DataObjects;
using Backend.Models;
using Microsoft.Azure.Mobile.Server;

namespace Backend.Controllers
{
    [Authorize]
    public class TodoItemController : TableController<TodoItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request);
        }

        // GET tables/TodoItem
        public IQueryable<TodoItem> GetAllTodoItems() => Query();

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TodoItem> GetTodoItem(string id) => Lookup(id);

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch) => UpdateAsync(id, patch);

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            TodoItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTodoItem(string id) => DeleteAsync(id);
    }
}
```

We could also have a version where reading was possible anonymously but updating the
database required authentication:

```csharp
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Backend.DataObjects;
using Backend.Models;
using Microsoft.Azure.Mobile.Server;

namespace Backend.Controllers
{
    public class TodoItemController : TableController<TodoItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request);
        }

        // GET tables/TodoItem
        public IQueryable<TodoItem> GetAllTodoItems() => Query();

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TodoItem> GetTodoItem(string id) => Lookup(id);

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch) => UpdateAsync(id, patch);

        // POST tables/TodoItem
        [Authorize]
        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            TodoItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        [Authorize]
        public Task DeleteTodoItem(string id) => DeleteAsync(id);
    }
}
```

Note that the `[Authorize]` attribute can do much more than what is provided
here.  Underneath there are various parameters that you can adjust to see if the
user belongs to a specific group or role.  However, the token that is checked
to see if the user is authenticated does not pull in any of the other information
that is normally needed for such authorization tasks.  As a result, the `[Authorize]`
tags is really only checking authentication - not authorization.

### Configuring an Authentication Provider

Configuration of the identity provider is very dependent on the identity provider
and whether the client is using a client-flow or server-flow.  Choose one of the
several options for authentication:

* [Enterprise Authentication][int-enterprise] covers Azure Active Directory.
* [Social Authentication][int-social] covers Facebook, Google, Microsoft and Twitter.

We also have Custom Authentication, which covers other techniques including sign-up /
sign-in, username and password and third party tokens.  We will cover that later.

### <a name="configreturn"></a>Testing Authentication without a Client

Testing your site without a client requires a REST client.  I use [Postman][19],
which is based on Google Chrome.  If you use Firefox, you might want to take
a look at [RESTClient][20].  Telerik also distributes a web debugging proxy
called [Fiddler][21] that can do API testing.  To test the server, we will need
a token.  We can get one by testing authentication configuration by pointing
the browser to `/.auth/login/_provider_` (replace _provider_ with your identity
provider).  The return URL will contain a token.

We can then do a request to `/tables/todoitem` to try and obtain the list of
current tasks.  We will need to add two headers:

* `ZUMO-API-VERSION` should contain a value of `2.0.0`.
* `X-ZUMO-AUTH` should contain the token you received.

My first request shows authentication failing:

![Authentication Failing][img30]

Go through one of the authentication flows and copy the authentication token.
In Postman, add a new header called `X-ZUMO-AUTH` and paste the authentication
token in.

![Authentication Success][img31]

Note that we have tested all this without touching the client.  Separating the
backend operations from the client operations means we can be sure of where
the inevitable bug that creeps in is located.  We have verified that we
can do each authentication flow on the server side and that the server is
properly rejecting unauthenticated requests, plus it is properly returning
data when authenticated requests are issued.

## Adding Authentication to a Mobile Client

Now that the backend is completely configured, we can move our attention to the
mobile client.  We are going to be using the same mobile client that we developed
in the first chapter, but we are now going to add authentication to it.
Web views are one of those items that are platform dependent. Fortunately for us,
Xamarin has already thought of this and provided a facility for running platform
specific code called the [DependencyService][22].

> If we run our application right now, clicking on the "Enter the App" button
will result in an error.  You will be able to see the Unauthorized error in the
debug window of Visual Studio.

Our first step is to define an `Abstractions\ILoginProvider.cs` interface
within the  shared project:

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

Next, we are going to extend our `Abstractions\ICloudService.cs` interface so
that the main application can call the login routine:

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

Our code will call `LoginAsync()` in the `ICloudService`, which will get the
platform-specific version of the login provider and call `LoginAsync()` there,
but with our defined mobile service client.  That is defined in the
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

The method looks up the platform dependent version of the login provider and
executes the login method, passing along the client (which we will need later).

### Server-Flow Authentication

In each platform-specific project, we are going to define a concrete
implementation of the login provider that uses a web view to hold the actual
authentication flow.  Here is the droid `Services\DroidLoginProvider.cs` (in the
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
            await client.LoginAsync(context, "google");
        }
    }
}
```

Let us take a closer look at this implementation.  The `LoginAsync()` method on
the Azure Mobile Apps client object takes the Android context (which is normally
the main window) and a provider - we can pick any of "facebook", "google",
"microsoftaccount", "twitter" or "aad" since we have defined all of them in the
Azure Portal configuration for our app service.  The clever piece is the
`Xamarin.Forms.Dependency` call at the top - that registers the class as a
platform service so we can access it through the Xamarin dependency service.

Note that we need an extra initialization routine for Android that must be
called prior the login provider being called to pass along the main window of
the app (also known as the context).  This is done in the `MainActivity.cs` file
**after** the Xamarin Forms initialization call.  The dependency service is not
set up until after the Xamarin Forms library is initialized, so we will not be
able to get the login provider reference before that point:

```csharp
protected override void OnCreate(Bundle bundle)
{
    base.OnCreate(bundle);

    Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

    global::Xamarin.Forms.Forms.Init(this, bundle);

    var loginProvider = (DroidLoginProvider)DependencyService.Get<ILoginProvider>();
    loginProvider.Init(this);

    LoadApplication(new App());
}
```

iOS is similar, but does not require the initialization step in the main startup
class.  The login provider class is in `Services\iOSLoginProvider.cs` (in the
**TaskList.iOS** project):

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
            var rootView = UIApplication.SharedApplication.KeyWindow.RootViewController;
            await client.LoginAsync(rootView, "google");
        }
    }
}
```

Note that we are using the same pattern here for registering the concrete
implementation with the dependency service, so we can get it the same way.
Finally, here is the UWP `Services\UWPLoginProvider.cs` (in the TaskList.UWP
project):

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
            await client.LoginAsync("google");
        }
    }
}
```

Now that we have all the platform-specific login routines registered, we can
move on to adding the login routine to the UI.  We have already got a button on
the entry page to enter the app.  It makes sense to wire up that button so
that it logs us in as well. The Command for the login button is in the
`ViewModels\EntryPageViewModel.cs`:

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

> The `ServiceLocator` class is my basic singleton handler.  It is available
in the [Chapter2][10] project.  It returns the concrete version of the cloud
service, just like the Singleton version we defined in Chapter1.

When you run the application, clicking on the "Enter the App" button will now
present you with an Authenticate window:

![Google Authenticate][img32]

Going through the authentication process will get you to the task list again.
If the authentication process fails, then `LoginAsync()` will throw an error,
which is caught at the ViewModel.  Right now, the `EntryPageViewModel` does
nothing more than print a diagnostic message to the debug window of Visual
Studio.

### What is in a JWT

At this point you will have the "Authentication Success" screen - perhaps several
times.  If you bring up the Developer Tools for your browser, you can take a look at
the token that is being minted for the authentication session.  Take a look at
the URL on the "successful authentication" page.

![The JWT][img27]

The authentication token is clearly marked (after you strip away the URL encoding).
You can use a [URL Decoder / Encoder][16] - just cut and paste the entire URL
into the box and click on **Decode**.  Note that the token is actually a JSON
object.  You can now easily extract the **authenticationToken** field from the
JSON object.

![The JWT Revealed][img28]

Technically, the authentication token is a [JSON Web Token][17].  This is a
mechanism for transferring claims between two systems securely.  The JWT is
a cryptographically signed JSON object.  You can decode the JWT using the
[jwt.io tool][18].  Cut and paste the authentication token into the **Encoded**
box and it will be decoded.

![The JWT Decoded][img29]

Note that the contents of the JWT are revealed even without knowing the secret.
However, we have not supplied a secret.  The secret is kept at the resource -
in this case, your app service.  However, we can already see the issuer and
audience (in this case, they are both set to your app service address), the
IdP that was used and a subject.

Technically, the JWT can include any data and there are some that place just
about everything about the user in the JWT.  App Service keeps the amount of
data small because the client will be sending the JWT with every request.
Imagine adding a few kilobytes to every single request that the client makes.
The bandwidth usage will add up quickly, and your app will be known as a
bandwidth hog.

However, there are some fields that are pretty universal.  Your JWT should
always have the following fields:

* sub = Subject (the identifier for the token)
* exp = Expiry (when the token expires)
* nbf = Not Before (the earliest point in time the token is valid)
* iss = Issuer (the site that issued the token)
* aud = Audience (who is the token for)

The timestamps (exp and nbf) are all UNIX timestamps (i.e. the number of
seconds since January 1, 1970).

App Service adds to this:

* stable_sid = Security Id of the user
* idp = the IdP that was used in the authentication request
* ver = the Version of the token

App Service will be able to validate any token provided to it when presented
in an X-ZUMO-AUTH header.  If you are using Azure Active Directory, you can
also use the more standard Bearer Authorization header.  If the token does not
match, then the X-ZUMO-AUTH header will be stripped from the request before
the request is passed to your site.

## Implementing Client-Flow

Now that server-flow is configured, we can move on to client-flow.

* [Enterprise Authentication][enterprise-clientflow]
* [Social Authentication][social-clientflow]
* [Custom Authentication][int-custom]

Custom Authentication is always based on a client-flow.   It covers username
and password implementations from a database, Azure Active Directory B2C for
sign-in and sign-up processes and accepting third-party tokens.  In short,
anything that doesn't fit into the five supported providers is custom.











## Developing Locally

One would normally be able to run the ASP.NET backend locally and get full functionality
without authentication.  However, authentication puts a stop to that because the redirect
URLs, secrets and other authentication configuration settings only work with a known
endpoint.  To alleviate that, Azure Mobile Apps allows you to run a local server while
using an authentication endpoint in Azure App Service.  When the authentication transaction
takes place, it is taking place against the Azure App Service.   When it is not doing
the OAuth transaction, however, it is operating against a local server.

Setting this up requires a little bit of local machine configuration and a change to
the configuration of your client.

### Update your Local Development Environment

The first step in this process is to make your local IIS development environment look
more like the Azure App Service, particularly in reference to the authentication settings.
This means setting up a few app settings that should be pulled from your App Service.

* Log on to the [Azure Portal][portal].
* Select your App Service from the **App Services** list.
* Click on **Tools**, then **Kudu**, then **Go**.

Kudu is the backend debug console for Azure App Service and there is a lot you can do
here.  Of note in this instance is that you can gain access to the keys and audience
for your App Service.

* Click on **Environment** in the top banner.
* Click on **Environment variables**.
* Scroll down to the environment variables starting with **WEBSITE\_AUTH**.
* Make a note of the **WEBSITE\_AUTH\_SIGNING\_KEY** and **WEBSITE\_AUTH\_ALLOWED\_AUDIENCES** values.

Add the following to your project Web.config `<appSettings>` section:

```xml
  <appSettings>
    <add key="PreserveLoginUrl" value="true" />
    <add key="MS_SigningKey" value="Overridden by portal settings" />
    <add key="EMA_RuntimeUrl" value="Overridden by portal settings" />
    <add key="MS_NotificationHubName" value="Overridden by portal settings" />
    <add key="SigningKey" value="{Your WEBSITE_AUTH_SIGNING_KEY}"/>
    <add key="ValidAudience" value="{Your WEBSITE_AUTH_ALLOWED_AUDIENCES}"/>
    <add key="ValidIssuer" value="https://{Your WEBSITE_HOSTNAME}/"/>
  </appSettings>
```

> **NOTE**: Both the ValidAudience and ValidIssuer will have a slash on the end and be a https URL.

The last three keys are the keys you will need to add.  Make sure you do not have a `HostName` key
as this is how the startup file determines if you are running locally or remote. Talking of whic,
edit your `App_Start\Startup.MobileApp.cs` file to include the following:

```csharp
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            new MobileAppConfiguration()
                .AddTablesWithEntityFramework()
                .ApplyTo(config);

            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer(new MobileServiceInitializer());

            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            if (string.IsNullOrEmpty(settings.HostName))
            {
                app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
                {
                    // This middleware is intended to be used locally for debugging. By default, HostName will
                    // only have a value when running in an App Service application.
                    SigningKey = ConfigurationManager.AppSettings["SigningKey"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
                    ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
                    TokenHandler = config.GetAppServiceTokenHandler()
                });
            }

            app.UseWebApi(config);
        }
```

The `UserAppServiceAuthentication()` method sets up authentication checking.  This section is not
required when running within App Service.

If you are running the server locally, you should either set up a local SQL Server instance and
put the connection string into the `Web.config` file, or [open the firewall on your SQL Azure][29]
database so that your local development environment can connect to it, then place the connection
string in the `Web.config`.  You can get the connection string of the SQL Azure instance by looking
at the Connection Strings in the **Application properties** of your App Service.

### Update your Mobile Client

For this demonstration, I have updated the **TaskList.UWP** application so that it is using the
server-flow authentication for Azure Active Directory.  This means updating the `LoginAsync()`
method in the `Services\UWPLoginProvider.cs` file to be the following:

```csharp
        public async Task LoginAsync(MobileServiceClient client)
        {
            // Server-Flow Version
            await client.LoginAsync("aad");
        }
```

This is because the default local IIS instance is IIS Express.  IIS Express only listens for local
connections.  If you run a client from another device (for example, the Android emulator on a Hyper-V
service or the iOS simulator on a Mac), then that client would be connecting via a network connection.
You can still debug locally, but you need to [convert your environment to IIS][28] first.

In the **TaskList (Portable)** project, update the `Helpers\Locations.cs` file:

```csharp
namespace TaskList.Helpers
{
    public static class Locations
    {
#if DEBUG
        public static readonly string AppServiceUrl = "http://localhost:17568/";
        public static readonly string AlternateLoginHost = "https://the-book.azurewebsites.net";
#else
        public static readonly string AppServiceUrl = "https://the-book.azurewebsites.net";
        public static readonly string AlternateLoginHost = null;
#endif
    }
}
```

The `AppServiceUrl` is always set to the location of your backend.  In this case, I right-clicked on
the `Backend` project and selected **Properties** then **Web**.  The correct URL for local debugging
is listed in the **Project URL**.  The `AlternateLoginHost` is set to the App Service when locally
debugging or null if not. You can specify the `DEBUG` constant in the **Build** tab.

In the same project, update the `Services\AzureCloudService.cs` constructor to the following:

```csharp
        public AzureCloudService()
        {
            client = new MobileServiceClient(Locations.AppServiceUrl);
            if (Locations.AlternateLoginHost != null)
                client.AlternateLoginHost = new Uri(Locations.AlternateLoginHost);
        }
```

> It's a good idea to separate the client and server into different solutions.  Although it
doesn't hurt anything to have them in the same solution (like we have), having the client
and server separated allows you to attach a debugger separately - which allows you to debug
both sides of the connection at the same time.

With these settings, the client will contact the AlternateLoginHost listed for the authentication
process and then contact the local server for the rest of the transaction.

### Run the Local Server

Running the local server and the client takes a larger machine.  You need to run two instances of
Visual Studio: one for the client and one for the server. This is really where you will appreciate
multiple monitors (my personal favorite) or the snap action to the sides of the screens.

Ensure you have your backend and clients in different solutions if you intend to run both client
and server.  The debugger in Visual Studio will stop one to run the other when they are in the same
solution.

There is still plenty to know:

* [Learn about User Claims and Authorization](./claims.md)
* [Learn about Refresh Tokens and Logging Out](./refresh.md)
* [Learn about the Best Practices](./best_practices.md)
## Obtaining User Claims

At some point you are going to need to deal with something other than the claims that are
in the token passed for authentication.  Fortunately, the Authentication / Authorization feature
has an endpoint for that at `/.auth/me`:

![The /.auth/me endpoint][img57]

Of course, the `/.auth/me` endpoint is not of any use if you cannot access it.  The most use
of this information is gained during authorization on the server and we will cover this use
later on.  However, there are reasons to pull this information on the client as well.  For
example, we may want to make the List View title be our name instead of "Tasks".

Since identity provider claims can be anything, they are transferred as a list within a JSON
object.  Before we can decode the JSON object, we need to define the models.  This is done
in the shared **TaskList** project.  I've defined this in `Models\AppServiceIdentity.cs`.

```csharp
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TaskList.Models
{
    public class AppServiceIdentity
    {
        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }

        [JsonProperty(PropertyName = "provider_name")]
        public string ProviderName { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "user_claims")]
        public List<UserClaim> UserClaims { get; set; }
    }

    public class UserClaim
    {
        [JsonProperty(PropertyName = "typ")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "val")]
        public string Value { get; set; }
    }
}
```

This matches the JSON format from the `/.auth/me` call we did earlier.   This is going to be
a part of the ICloudService as follows:

```csharp
using System.Threading.Tasks;
using TaskList.Models;

namespace TaskList.Abstractions
{
    public interface ICloudService
    {
        ICloudTable<T> GetTable<T>() where T : TableData;

        Task LoginAsync();

        Task LoginAsync(User user);

        Task<AppServiceIdentity> GetIdentityAsync();
    }
}
```

Finally, we need to actually implement the concrete version in `AzureCloudService.cs`:

```csharp
        List<AppServiceIdentity> identities = null;

        public async Task<AppServiceIdentity> GetIdentityAsync()
        {
            if (client.CurrentUser == null || client.CurrentUser?.MobileServiceAuthenticationToken == null)
            {
                throw new InvalidOperationException("Not Authenticated");
            }

            if (identities == null)
            {
                identities = await client.InvokeApiAsync<List<AppServiceIdentity>>("/.auth/me");
            }

            if (identities.Count > 0)
                return identities[0];
            return null;
        }
```

Note that there is no reason to instantiate your own `HttpClient()`.  The Azure Mobile Apps
SDK has a method for invoking custom API calls (as we shall see later on).  However, if you
prefix the path with a slash, it will execute a HTTP GET for any API with any authentication
that is currently in force.  We can leverage this to call the `/.auth/me` endpoint and decode
the response in one line of code.

We can adjust the `ExecuteRefreshCommand()` method in the `ViewModels\TaskListViewModel.cs`
file to take advantage of this:

```csharp
        async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var identity = await cloudService.GetIdentityAsync();
                if (identity != null)
                {
                    var name = identity.UserClaims.FirstOrDefault(c => c.Type.Equals("name")).Value;
                    Title = $"Tasks for {name}";
                }
                var list = await Table.ReadAllItemsAsync();
                Items.ReplaceRange(list);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Items Not Loaded", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
```

The return value from the `GetIdentityAsync()` method is the first identity.  Normally, a user would
only authenticate once, so this is fairly safe.  The number of claims returned depends on the identity
provider and could easily number in the hundreds.  Even the default configuration for Azure Active
Directory returns 18 claims.  These are easily handled using LINQ, however.  The `Type` property holds
the type.  This could be a short (common) name.  It could also be a schema name, which looks more like
a URI.  The only way to know what claims are coming back for sure is to look at the `/.auth/me`
result with something like Postman.

> **Note**: If you are using Custom Authentication (e.g. username/password or a third-party token),
then the `/.auth/me` endpoint is not available to you.  You can still produce a custom API in your
backend to provide this information to your client, but you are responsible for the code - it's
custom, after all!

## Authorization

Now that we have covered all the techniques for authentication, it's time to look at
authorization.  While authentication looked at verifying that a user is who they say
they are, authorization looks at if a user is allowed to do a specific operation.

Authorization is handled within the server-side project by the `[Authorize]` attribute.
Our Azure Mobile Apps backend is leveraging this to provide authorization based on
whether a user is authenticated or not.  The Authorize attribute can also check to see
if a user is in a list of users or roles.  However, there is a problem with this.  The
user id is not guessable and we have no roles.  To see what I mean, run the **Backend**
project locally and set a break point on the `GetAllTodoItems()` method in the
`TodoItemController`, then run your server and your UWP application.

> Once you have built and deployed the UWP application, it will appear in your normal Application
list.  This allows you to run the application and the server at the same time on the same
machine.

Once you have authenticated, you will be able to set a break point to take a look at
`this.User.Identity`:

![this.User.Identity output][img55]

Note that the `Name` property is null.  This is the property that is used when you want to
authorize individual users.  Expand the `Claims` property and then click on **Results View**:

![Claims output][img56]

The only claims are the ones in the token, and none of them match the `RoleClaimType`, so we
can't use roles either.  Clearly, we are going to have to do something else.  Fortunately, we
already know that we can get some information about the identity provider claims from the
`/.auth/me` endpoint.  To get the extra information, we need to query the `User` object:

```csharp
var identity = await User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(Request);
```

There is one `Credentials` class for each supported authentication technique - Azure Active Directory, Facebook,
Google, Microsoft Account and Twitter.  These are in the **Microsoft.Azure.Mobile.Server.Authentication**
namespace.  They all follow the same pattern as the model we created for the client - there are Provider,
UserId and UserClaims properties.  The token and any special information will be automatically decoded
for you.  For instance, the TenantId is pulled out of the response for Azure AD.

> You can use the AccessToken property to do Graph API lookups for most providers in a custom API.
We'll get into this more in a later Chapter.

## Refresh Tokens

### Configuring Refresh Tokens

### Using Refresh Tokens

## Logging out

## Best Practices

[img1]: img/auth-flow.PNG
[img27]: img/jwt-1.PNG
[img28]: img/jwt-2.PNG
[img29]: img/jwt-3.PNG
[img30]: img/testing-auth-failed.PNG
[img31]: img/testing-auth-success.PNG




[portal]: https://portal.azure.com/
[classic-portal]: https://manage.windowsazure.com/
[int-enterprise]: ./enterprise.md
[int-social]: ./social.md
[int-custom]: ./custom.md
[enterprise-clientflow]: ./enterprise.md#clientflow
[social-clientflow]: ./social.md#clientflow


[1]: https://en.wikipedia.org/wiki/Multi-factor_authentication
[2]: https://support.apple.com/en-us/HT201371
[3]: http://oauth.net/2/
[4]: https://developer.linkedin.com/docs/oauth2
[5]: https://developer.github.com/v3/oauth/
[6]: https://auth0.com/
[7]: https://azure.microsoft.com/en-us/services/active-directory-b2c/

[16]: http://meyerweb.com/eric/tools/dencoder/
[17]: https://openid.net/specs/draft-jones-json-web-token-07.html
[18]: https://jwt.io
[19]: https://www.getpostman.com/
[20]: https://addons.mozilla.org/en-US/firefox/addon/restclient/
[21]: http://www.telerik.com/blogs/api-testing-with-telerik-fiddler
[22]: https://developer.xamarin.com/guides/xamarin-forms/dependency-service/
[23]: http://www.asp.net/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2
[24]: https://azure.microsoft.com/en-us/services/active-directory-b2c/
[25]: https://developer.github.com/v3/oauth/
[26]: http://www.miicard.com/for/individuals/how-it-works
[27]: https://www.auth0.com/
[28]: https://github.com/Azure/azure-mobile-apps-net-server/wiki/Local-development-and-debugging-the-Mobile-App-.NET-server-backend
[29]: https://azure.microsoft.com/en-us/documentation/articles/sql-database-configure-firewall-settings/
