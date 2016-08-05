# Enterprise Authentication

Enterprise Authentication is handled by Azure Active Directory.  This is
a fairly common Azure service.  Every Azure subscription has a default
directory associated with it that you can leverage for this section.  In
addition, if your organization has an Office 365 subscription, this will
likely be tied to an Azure Active Directory domain to allow enterprise
sign-in.  In either case, you have a directory you can use for providing
authentication to your app.

In general, you will need to get special permissions to update the directory.
If you want to use your organizations corporate directory, then you are likely
to have to get your IT department involved to set it up.

## Server-Flow setup

The Azure Active Directory server-flow is perhaps the easiest of all the
authentication methods to configure.  No matter if you are doing a client
flow or server flow, you need to set up the server flow first.

> We recommend that you implement Client Flow in any non-trivial application.

If you are using your default directory and you want to add  a couple of
test users, you will need to set those up first.   Start by going to the
[Classic Azure Portal][classic-portal]:

![Classic Portal][img24]

Click on the **Default Directory**, then click on **USERS**.  You will notice
that your Azure-linked ID is already present.

![AzureAD: Users][img25]

Click on **Add User** at the bottom of the screen.  Enter a username in the
box provided before clicking on the arrow.  Then fill in the personal
information and click on the arrow again.  Finally, click on **create**.
Note the password, before clicking on the tick.  Now you have two users - your
Azure ID and the one you just created.  Note the username - it will be something
like `adrian@photoadrianoutlook.onmicrosoft.com` - it is a little unwieldy.

To configure your app, switch back to the regular [Azure Portal][portal], find
your App Service, click on **All Settings** followed by **Authentication / Authorization**.
Finally, select **Azure Active Directory**.

![AzureAD: Configuration][img26]

Click on **Express**.  Note that all the information is filled in for you.  All
you have to do is click on **OK**, followed by **Save**.

> Make sure you create the app service in the right directory / subscription.
If you have access to more than one directory, you can choose the right one by
selecting it under your account drop-down in the top-right corner.

There is also an **Advanced** track.  This is used in client-flow situations
and in situations where you have more than one directory.  The Express flow
is great for getting started quickly.

You can walk through a server-flow authentication to test that you have all the
settings correct.  Point your browser at https://_yoursite_.azurewebsites.net/.auth/login/aad.
The browser will take you through an authentication flow before giving you a
successful authentication image:

![AzureAD: Success][img7]

## Client-Flow Setup

Configuring Azure Active Directory for client-flow is a three-step process.  First,
we need to create a WEB application.  This represents the resource: in our case, the
resource is the Azure Mobile Apps backend.  Then we need to create a NATIVE application.
This represents the client: in our case, the ADAL (Active Directory Access Library) library
will need this information.  Finally, we need to give the NATIVE application permission
to access the WEB application.

It starts with configuring a server-flow to protect the resource.  We've already done that
above. Then configure a "Native Application" and give it permissions to the web application:

* Log on to the [Classic Portal][classic-portal].
* Select the **Default Directory** from your list of all items.
* Click on the **APPLICATIONS** tab.

  ![Azure AD Apps][img32]

* Note that our existing web application is already there.  You will see more applications,
  depending on what you have set up.  In this example, I have Visual Studio Team Services
  and Office 365 set up.
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

* At the bottom of the page is the **permissions to other applications** section.  Click on the
  **Add application** button.

  ![Azure AD Apps - Permissions (1)][img38]

* Click on the **SHOW** drop-down and select **All Apps**, then click on the tick next to the search box.
* Click on the web application that you set up during the server-flow configuration, then click on the
  tick in the lower-right corner.

  ![Azure AD Apps - Permissions (2)][img39]

* Click on **Delegated Permissions** next to the web application.  Check the box next to **Access*, then
  click on **Save** at the bottom of the screen.

  ![Azure AD Apps - Permissions (3)][img40]

At this point the application configuration will be saved.

So, what did we just do there?  We created a new Azure AD app for the native application.  We
then gave permission for the native application to access resources that are protected by
the web application.  In our Azure App Service, we configured the service so that the
Azure AD web application is used to protect our resources.  The net effect is that our
native application OR our web application can access the App Service resources that are
protected via the `[Authorize]` attribute.

> I'm going to assume you have [returned to the Concepts page][int-concepts] and added the
client code for a server flow.

### <a name="clientflow"></a>The Code for Client Flow

Before continuing, you will need the **Client ID** and the **Redirect URI** for the application.
You can enter these into the `Helpers\Locations.cs` file in the shared project:

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

The **AadClientId** and **AadRedirectUri** must match what you have configured in Azure AD for
your native app.  The other piece of information you need to add is the Azure AD Authority for your
directory.  If you click on the **DOMAINS** tab, it will generally tell you what domain you are in.
The Authority is just a path on the `https://login.windows.net` that corresponds to your domain.
There is also a GUID version of this domain.  You can find the GUID by looking at the **View Endpoints**
in the **APPLICATIONS** tab.  Look at the first path section of most all the endpoints.

Add the **Microsoft.IdentityModel.Clients.ActiveDirectory** NuGet package using **Manage NuGet Packages...**
to each platform project.  This package contains the ADAL library as a portable class library.

![Azure AD - Add ADAL Library][img41]

Now you can add the client flow to each project.  Start with the login provider in the **TaskList.UWP**
project, located in the `Services\UWPLoginProvider.cs` file:

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

The `LoginADALAsync()` method does the actual client-flow - using the ADAL library to
authenticate the user and return the access token.  The `LoginAsync()` method initiates
the client-flow.  It uses the token it receives from the client-flow to log in to the
App Service, by packaging the token into a JSON object.  I have placed the client and
server flow next to each other so you can compare the two.

In the **TaskList.Droid** project, we need to deal with the `Context`, as is common
with Android libraries.  The client flow in `Services\DroidLoginProvider.cs` is
remarkably similar though:

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

The only real difference between this one and the Universal Windows edition is the
PlatformParameters. We need to pass in the context of the MainActivity (which is passed
in through the `Init()` call).  However, we must also handle the response from the ADAL
library.  This is done in `MainActivity.cs`. Add the following method to the `MainActivity` class:

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

Note that we can balance the needs of each platform by using the dependency service.  The code that
is unique to the platform is minimized and stored with the platform.

## <a name="addlclaims"></a>Adding Additional Claims to the Request

There are times when you want to add soemthing else to the token that is returned from Azure AD. The
most common request is to add group information to the response so you can handle group-based
authorization.

To add security groups to the Azure AD token:

1. Log into the [Classic Portal][classic-portal].
2. Click on your directory (probably called **Default Directory**) in the **All Items** list.
3. Click on **APPLICATIONS**, then your WEB application.
4. Click on **MANAGE MANIFEST** (at the bottom of the page), then **Download Manifest**.
5. Click on **Download manifest**.

This will download a JSON file.  Edit the file with a text editor.  (I use Visual Studio Code).  At
the top of the file is this:

```json
  "displayName": "webapp-for-the-book",
  "errorUrl": null,
  "groupMembershipClaims": null,
  "homepage": "https://the-book.azurewebsites.net",
  "identifierUris": [
    "https://the-book.azurewebsites.net"
  ],
  "keyCredentials": [],
  "knownClientApplications": [],
```

Change the **groupMembershipClaims** to "SecurityGroup":

```json
  "displayName": "webapp-for-the-book",
  "errorUrl": null,
  "groupMembershipClaims": "SecurityGroup",
  "homepage": "https://the-book.azurewebsites.net",
  "identifierUris": [
    "https://the-book.azurewebsites.net"
  ],
  "keyCredentials": [],
  "knownClientApplications": [],
```

Save the file.  You can now upload this again.  Go back to the WEB application, click on **MANAGE MANIFEST**,
then click on **Upload Manifest**.  Select the file and click on the tick.

![AAD - Upload Manifest][img-upload-manifest]

You can now give the web application additional permissions:

1. Click on the **CONFIGURE** tab.
2. Scroll to the bottom, click on **Delegated Permissions**.
3. Check the box for **Read directory data**.

   ![AAD: Group Permissions][img-group-perms]

4. Click on **Save**.

Now that you have configured the application to return groups as part of the claims, you should
probably add a couple of groups:

1. Click on the back-arrow (at the top left) to return to the top level of your directory.
2. Click on **GROUPS**.
3. Click on **ADD GROUP**.
4. Fill in the information, select **Security** as the group type, then click on the tick.

   ![AAD: Add Group][img-add-group]

5. Click on the new group, then click on **PROPERTIES**.

   ![AAD: Group Properties][img-group-props]

6. Make a note of the **OBJECT ID**.  The claims for groups are listed by the Object ID, so you will
   need this to refer to the group later.

It's a good idea to add a couple of groups for testing purposes.  If you are using the organization
directory, you will need to request the creation of a couple of groups for application roles.

> Ready to continue?  Return to the [Concepts] to continue the journey!

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
[img-upload-manifest]: img/aad-upload-manifest.PNG
[img-group-perms]: img/aad-group-perms.PNG
[img-add-group]: img/aad-group-1.PNG
[img-group-props]: img/aad-group-2.PNG

[int-concepts]: ./authentication.md#configreturn
[portal]: https://portal.azure.com/
[classic-portal]: https://manage.windowsazure.com/
[Concepts]: ./authentication.md#clientreturn
