One of the very first things you will want to do is to provide users with a unique experience.  For our example
task list application, this could be as simple as providing a task list for the user who is logged in.  In more
complex applications, this is the gateway to role-based access controls, group rules, and sharing with your
friends.  In all these cases, properly identifying the user using the phone is the starting point.

Authentication provides a process by which the user that is using the mobile device can be identified securely.
This is generally done by entering a username and password.  However, modern systems can also provide
[multi-factor authentication][1], send you a text message to a registered device, or [use your fingerprint][2]
as the password.

## The OAuth Process

In just about every single mobile application, a process called [OAuth][3] is used to properly identify a user
to the mobile backend.  OAuth is not an authentication mechanism in its own right.  It is used to route the
authentication request to the right place and to verify that the authentication took place. There are three
actors in the OAuth protocol:

* The **Client** is the application attempting to get access to the resource.
* The **Resource** is the mobile backend that the client is attempting to access.
* The **Identity Provider** (or IdP) is the service that is responsible for authenticating the client.

At the end of the process, a cryptographically signed token is minted.  This token is added to every single
subsequent request to identify the user.

## Server Side vs. Client Side

There are two types of authentication flow: Server-Flow and Client-Flow.  They are so named because of who
controls the flow of the actual authentication.

![Authentication Flow][img1]

Server-flow is named because the authentication flow is managed by the server through a web connection.  It
is generally used in two cases:

* You want a simple placeholder for authentication in your mobile app while you are developing other code.
* You are developing a web app.

In the case of Server Flow:

1. The client brings up a web view and asks for the login page from the resource.
2. The resource redirects the client to the identity provider.
3. The identity provider does the authentication before redirecting the client
   back to the resource (with an identity provider token).
4. The resource validates the identity provider token with the identity provider.
5. Finally, the resource mints a new resource token that it returns to the client.

Client-flow authentication uses an IdP provided SDK to integrate a more native feel to the authentication
flow.  The actual flow happens on the client, communicating only with the IdP.

1. The client uses the IdP SDK to communicate with the identity provider.
2. The identity provider authenticates the user, returning an identity provider token.
3. The client presents the identity provider token to the resource.
4. The resource validates the identity provider token with the identity provider.
5. Finally, the resource mints a new resource token that it returns to the client.

For example, if you use the Facebook SDK for authentication, your app will seamlessly switch over into the
Facebook app and ask you to authorize your client application before switching you back to your client application.

You should use the IdP SDK when developing an app that will be released on the app store.  The identity providers
will advise you to use their SDK and it provides the best experience for your end users.

## Authentication Providers

Azure Mobile Apps supports five identity providers natively:

* Azure Active Directory
* Facebook
* Google
* Microsoft (MSA)
* Twitter

> Azure App Service Authentication / Authorization maintains a token store in the XDrive (which is the drive that is
shared among all instances of the backend within the same App Service Plan).  The token store is located at
`D:\\home\\data\\.auth\\tokens` on the backend.  The tokens are encrypted and stored in a per-user encrypted file.

In addition, you can set up client-flow custom authentication that allows you to mint a ZUMO token to your
specifications for any provider using a client-flow.  For example, you could use authentication providers
like [Azure AD B2C][7], [LinkedIn][4] or [GitHub][5], a third-party authentication provider like  [Auth0][6],
or you could set up an identity table in your database so that you can check  username and password without
an identity provider.

<!-- Images -->
[img1]: img/auth-flow.PNG

<!-- External Links -->
[1]: https://en.wikipedia.org/wiki/Multi-factor_authentication
[2]: https://support.apple.com/en-us/HT201371
[3]: http://oauth.net/2/
[4]: https://developer.linkedin.com/docs/oauth2
[5]: https://developer.github.com/v3/oauth/
[6]: https://auth0.com/
[7]: https://azure.microsoft.com/en-us/services/active-directory-b2c/
