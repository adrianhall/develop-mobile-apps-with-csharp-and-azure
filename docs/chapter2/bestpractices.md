## Best Practices

We've covered a lot of ground with authentication and authorization, so I wanted to cover some of the
best practices that I generally advise when thinking about this topic.

### Don't store passwords

I can't really advise on which identity provider is best for your mobile application.  However, I can
clearly say that delegating the security of the identity database to someone who has that as their full
time job is an excellent idea.

Choosing an identity provider is not easy.  Here are my choices:

1. If you need enterprise authentication, use **Azure Active Directory**.
2. If you need a specific social identity provider (for example, Facebook or Google), use that.
3. If you need multiple social identity providers, **Auth0** is an excellent choice.
4. If you need usernames and password, use **Azure Active Directory B2C**.

Storing usernames and passwords in your own database is a bad idea and should be avoided.

### Use the client SDK provided by the Identity Provider

Your first step should be getting an access token from the identity provider itself.  You will see the
most integrated experience if you use their SDK.

1. For **Azure Active Directory**, that SDK is [ADAL][1].
2. For **Facebook**, check out [Xamarin.Facebook.iOS][2] or [Xamarin.Facebook.Android][3].
3. For **Google**, check out [Google APIs Core Client Library][4].
4. For **Auth0**, check out the [Auth0 Xamarin Component][5].
5. For **Azure Active Directory B2C**, use [ADAL][1].

### Swap the Identity Provider access token for a ZUMO token.

Never use the token provided by the identity provider for anything other than requesting access to
the resource.  Use a short lived token (an hour is the standard) that is minted just for the purpose
of providing that access.  If you are not using an identity provider that is explicitly supported
by Azure App Service, use a custom authentication provider to mint your own token.

### Enforce Security at the Server

There is no easy way to say this.  There are bad guys out there, and they are after your data.  You
should not assume that someone is using your client.  It could just be someone with a REST client.
Ensure you enforce security on your server.  You can do this easily by using the `[Authorize]`
attribute, the `[AuthorizeClaims()]` attribute we developed in the Authorization section or your
own custom authorization attribute.

Monitoring the server is just as important as enforcing security.  The Azure App Service Authentication
service outputs quite a bit of logging about who is logging in (and who is denied), so you can get some
good intelligence out of the logs when you mine them properly.

### Securely Store Tokens

The decisions we make are always a trade-off between convenience and security.  One such decision is
if we should store tokens with the app.  On the one hand, it's convenient to allow the app to remember
the login and to not ask us to log in again with each app start.  On the other hand, that fact opens
up a security hole that a determined hacker can exploit.  We can have convenience while still having
security by utilizing the secure stores that each platform provides to store secrets like the access
token.

### Use https only

This should go without saying.  Always use https communication.  Most security professionals start off
their security career with learning "Security at Rest & Security in Transit".  In practical terms, storing
secrets (like tokens) securely and using HTTPS as a transport mechanism satisfies both claims.

Don't stop with security there though.  HTTPS is just a medium through which secure communications can
take place.  There are a wide range of protocols and ciphers that can be used to encrypt the traffic.
Some are  considered less secure and not to be used.  Azure App Service provides a default set of protocols
and ciphers to support backwards compatibility with older browsers.  You can adjust the ciphers in use
by your App Service.  For information on this, refer to the [Azure Documentation][10].

### Handle Expiring Tokens

Unless you are using an identity provider that doesn't support refresh tokens (like Facebook or Twitter),
you should handle refresh tokens by silently calling the refresh action.  Tokens are going to expire.
This is a fact of the token specifications.  You need to deal with expiring tokens and act accordingly.

If you do need to use an identity provider that does not support refresh tokens, you are going to have
to ask for credentials whenever the token expires.  You don't get out of determining the user experience
when tokens expire just because you are using Facebook or Twitter.

## Authenticating Your App

My final word on authentication has to do with authenticating your app.  I get the same request every
week.  How do I implement an API key for my app?  When I probe a little, I get a few reasons for this
request:

1. I want to ensure my app is the only one accessing my backend because the data is important.
2. I don't want my users to log in as it is inconvenient.
3. I want to monetize my app, and I can't do that if anyone can copy it.

API keys are used by multi-tenant systems to route requests for data to the appropriate data store.  For
example, the very popular [Parse Server][6] used to have an API key because all clients connected to the
same `parse.com` service.  Once the [Parse Server][6] was open-sourced, the API key went away.  It was no
longer needed to route the request.  In the same way, the Azure App Service has a unique name - the URL
of the service, so it doesn't need an API key to route the information.

An API key does not prevent a rogue client from accessing your data.  If you did use an API key for security,
you can easily get the API key for the app by putting together a "man in the middle proxy".  One such proxy
is [Telerik Fiddler][7].  One of its features is "Security Testing" which amounts to a man-in-the-middle
decryption technique.  This [works with the Android emulator][8] as well.  For iOS, you can use [Charles][9].

So, how do you authenticate your app?  Step back a moment.  What are you monetizing or protecting?  It's
likely the data within the mobile backend.  Protect that data by authenticating your users.   If you absolutely
must monetize your app, then there are ways to do it, and we will discuss those later in the book.

<!-- Links -->
[1]: https://www.nuget.org/packages/Microsoft.IdentityModel.Clients.ActiveDirectory/
[2]: https://www.nuget.org/packages/Xamarin.Facebook.iOS/
[3]: https://www.nuget.org/packages/Xamarin.Facebook.Android/
[4]: https://www.nuget.org/packages/Google.Apis.Core/
[5]: https://components.xamarin.com/view/auth0client
[6]: https://github.com/ParsePlatform/parse-server
[7]: http://www.telerik.com/fiddler
[8]: https://aurir.wordpress.com/2010/03/22/tutorial-getting-android-emulator-working-with-fiddler-http-proxy-tool/
[9]: https://www.charlesproxy.com/documentation/faqs/using-charles-from-an-iphone/
[10]: https://azure.microsoft.com/en-us/documentation/articles/app-service-app-service-environment-custom-settings/
