# Authentication

One of the very first things you will want to do is to provide users with a
unique experience.  For our example task list application, this could be as
simple as providing a task list for the user who is logged in.  In more complex
applications, this is the gateway to role-based access controls, group rules,
and sharing with your friends.  In all these cases, properly identifying the
user using the phone is the starting point.

## Authentication Concepts

Authentication provides a process by which the user that is using the mobile
device can be identified securely.  This is generally done by entering a username
and password.  However, modern systems can also provide [multi-factor authentication][1],
send you a text message to a registered device, or [use your fingerprint][2] as the password.

### The OAuth Process

In just about every single mobile application, a process called [OAuth][3] is used
to properly identify a user to the mobile backend.  OAuth is not an authentication
mechanism in its own right.  It is used to route the authentication request to the
right place and to verify that the authentication took place. There are three actors
in the OAuth protocol:

* The **Client** is the application attempting to get access to the resource.
* The **Resource** is the mobile backend that the client is attempting to access.
* The **Identity Provider** (or IdP) is the service that is responsible for authenticating the client.

At the end of the process, a cryptographically signed token is minted.  This
token is added to every single subsequent request to identify the user.

### Server Side vs. Client Side Authentication

There are two types of authentication flow: Server-Flow and Client-Flow.  They
are so named because of who controls the flow of the actual authentication.  In
a server-flow authentication, the client asks Azure Mobile Apps to login.  Azure
Mobile Apps then redirects to the configured Identity Provider.  That IdP will
then authenticate the user before redirecting back to Azure Mobile Apps.  At
this point, Azure Mobile Apps will mint a ZUMO token that shows the user is
authenticated and return that token to the client application.  

![Server-Flow][img1]

Server-flow is named because the authentication flow is managed by the server
through a web connection.  It is generally used in two cases:

* You want a simple placeholder for authentication in your mobile app while you are developing other code.
* You are developing a web app.

Client-flow authentication uses an IdP provided SDK to integrate a more native
feel to the authentication flow.  The actual flow happens on the client,
communicating only with the IdP.  For example, if you use the Facebook SDK for
authentication, your app will seamlessly switch over into the Facebook app and
ask you to authorize your client application before switching you back to your
client application.  The IdP SDK will return a token to your code.  Your client
application will then contact the mobile backend to swap the IdP token for a
ZUMO token.  You will then use the ZUMO token to communicate with the mobile
backend.

It is generally recommended that you use the IdP SDK when developing an app
that will be released on the app store.  This follows the best practice provided
by the majority of identity providers and provides the best experience for your
end users.

### Authentication Providers

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

Adding authentication to an Azure Mobile Apps backend is made easier because
Azure Mobile Apps adds authentication using the default ASP.NET identity
framework.  However, you must add the authentication initialization code to
your `Startup.MobileApp.cs` file:

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

Authentication is done at one of two levels.  We can add
authentication to an entire table controller by adding the `[Authorize]`
attribute to the table controller.  We can also add authentication on
individual operations by adding the `[Authorize]` attribute to individual
methods within the table controller. For example, here is our table controller
from the first chapter with authentication required for all operations:

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

We could also have a version where reading was possible anonymously but
updating the database required authentication:

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

Note that the `[Authorize]` attribute can do so much more than what is provided
here.  Underneath there are various parameters that you can adjust to see if the
user belongs to a specific group or role.  However, the token that is checked
to see if the user is authenticated does not pull in any of the other information
that is normally needed for such authorization tasks.  As a result, the `[Authorize]`
tags is really only checking authentication - not authorization.

### Social Authentication

Azure App Service provides built-in support for Facebook, Google, Microsoft
and Twitter.  Irrespective of whether you intend to use server-flow or
client-flow, you will need to configure the Azure App Service Authentication
service.  In general, the method involves:

1.  Obtain a Developer Account for the provider.
2.  Create a new application, obtaining a Client ID and Secret.
3.  Turn on Azure App Service Authentication.
4.  Enter the Client ID and Secret into the specific provider setup.
5.  Save the configuration.

Before you start any of this, create a new Azure Mobile Apps as we described
in [Chapter 1][int_1].  If you want a site to deploy for the configuration, the
**Backend** project in the [Chapter2][10] solution is pre-configured for authorization.
You just need to deploy it to Azure App Service.

#### Facebook Authentication

I'm going to assume you have a Facebook account already.  If you don't, go to
[Facebook][8] and sign up.  All your friends are likely there already!  Now
log in to the [Facebook Developers][9] web site.  Let's create a new Facebook
application:

![Facebook Developers][img2]

**Note**: Facebook updates the look and feel of their developer site on a regularb
basis.  As a result, the screen shots I've provided here may be different.  If
in doubt, follow the bullet descriptions to find your way.

> If you are not already registered, click on the drop-down in the top-right
corner and **Register as a Developer** before continuing.

* Click on the **My Apps** link in the top right corner of the screen.
* Click on **Create a New App**.
* Fill in the form:

![Create a new Application][img3]

* If required, verify your account according to the instructions.  This usually
involves adding a credit card number or verifying your mobile phone number.  

* Click on the **Get Started** button next to **Facebook Login**.

![Facebook Login][img4]

* Enter your application URL + `/.auth/login/facebook/callback` in the **Valid
OAuth redirect URIs**.

![Facebook OAuth redirect URIs][img5]

> Note that you may not be able to use SSL if you are using the certain plans such
as the F1 Free App  Service Plan.  Some identity providers only allow SSL redirects.  
You can upgrade the App Service Plan to a B1 Basic in this case.

* Click on **Save Changes**.
* Click on the **Settings** -> **Basic** in the left hand side-bar.
* Click on the **Show** button next to the App Secret

Now that you have the **App ID** and **App Secret**, you can continue configuration
of your app within the [Azure Portal][portal].

* Open up your App Service by clicking on **All Resources** or **App Services**
followed by the name of your app service.
* In the **Settings** blade, click on **Authentication / Authorization** which
is under **Features**.
* Turn **App Service Authentication** to **On**.
* In the **Action to take when request is not authenticated**, select **Allow Request (no action)**.

> It's very tempting to choose **Log in with Facebook**.  However, you need to avoid this.  Selecting this option will mean that all requests need to be authenticated and you won't get the information on the back end.  Selecting **Allow Request** means your app is in charge of what gets authenticated and what does not require authentication.

* Click on **Facebook** (which should show _Not Configured_).
* Cut and Paste the **App ID** and **App Secret** into the boxes provided.
* Select **public_profile** and **email** for Scopes.

> Note that if you request anything but public_profile, user_friends, and email, your app will need further review by Facebook, which will take time.  This process is not worth it for test apps like this one.

* Click on **OK** (at the bottom of the blade) to close the Facebook configuration blade.
* Click on **Save** (at the top of the blade) to save your Authentication changes.

You can test your authentication process by browsing to https://_yoursite_.azurewebsites.net/.auth/login/facebook;
this is the same endpoint that the Azure Mobile Apps Client SDK calls when it is time
to integrate authentication into the mobile client.

![Confirming Facebook Authentication][img6]

If you are not logged in to facebook already, you will be prompted for your
facebook credentials first.  Finally, here is your happy page - the page that
signifies you have done everything right:

![Authentication Succeeded][img7]

#### Google Authentication

### Enterprise Authentication

### What is in a JWT

## Implementing Authentication to a Mobile Client

### Social Authentication

### Enterprise Authentication

## Custom Authentication

### Azure Active Directory B2C

### Using Third-Party Tokens

### Using an Identity Database.

## Authorization

## Refresh Tokens

### Configuring Refresh Tokens

### Using Refresh Tokens

## Logging out

## Best Practices

[img1]: img/ch2/idp-flow.PNG
[img2]: img/ch2/fb-dev-1.PNG
[img3]: img/ch2/fb-dev-2.PNG
[img4]: img/ch2/fb-dev-3.PNG
[img5]: img/ch2/fb-dev-4.PNG
[img6]: img/ch2/fb-dev-5.PNG
[img7]: img/ch2/auth-success.PNG

[int_1]: 1_introduction.md
[portal]: https://portal.azure.com/

[1]: https://en.wikipedia.org/wiki/Multi-factor_authentication
[2]: https://support.apple.com/en-us/HT201371
[3]: http://oauth.net/2/
[4]: https://developer.linkedin.com/docs/oauth2
[5]: https://developer.github.com/v3/oauth/
[6]: https://auth0.com/
[7]: https://azure.microsoft.com/en-us/services/active-directory-b2c/
[8]: https://facebook.com/
[9]: https://developers.facebook.com/
