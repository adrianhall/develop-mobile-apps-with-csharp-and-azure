## Caching Tokens

You will notice that we have to log in with every start of the application.  The token that is generated has a lifetime
that is provided and controlled by the identity provider.  Some providers have a relatively short lifetime.  For example,
Azure Active Directory tokens have a lifetime of 1 hour.  Others are incredibly long.  Facebook has an expiry time of
60 days.

Irrespective of the lifespan of the token, we will want to store it securely and re-use it when we can.  Xamarin has
provided a nice component, [Xamarin.Auth][30], that provides such as secure store in a cross-platform manner.  It starts
with an account store:

```csharp
// For iOS:
var accountStore = AccountStore.Create();
// For Android:
var accountStore = AccountStore.Create(Context);
```

We can then store the token with the following:

```csharp
accountStore.Save(account, "descriptor");
```

The descriptor is a string that allows us to find the token again.  The account (which is an `Account` object) is
uniquely identified by a key composed of the account's Username property and the descriptor.  The Account class is
provided with Xamarin.Auth.  Storage is backed by the [Keychain] on iOS and the [KeyStore] on Android.

To get the token back, we use the following:

```csharp
var accounts = accountStore.FindAccountsForService("descriptor");
```

When we receive the token back from the key store, we will want to check the expiry time to ensure the token has not
expired.  As a result, there is a little bit more code to caching code than one would expect.

Let's start with the Android version in **TaskList.Droid**.  As with all the other login code, we are adjusting the
`LoginAsync()` method in `Services\DroidLoginProvider.cs`:

```csharp
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Droid.Services;
using TaskList.Helpers;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(DroidLoginProvider))]
namespace TaskList.Droid.Services
{
    public class DroidLoginProvider : ILoginProvider
    {
        public Context RootView { get; private set; }

        public AccountStore AccountStore { get; private set; }

        public void Init(Context context)
        {
            RootView = context;
            AccountStore = AccountStore.Create(context);
        }

        public async Task LoginAsync(MobileServiceClient client)
        {
            // Check if the token is available within the key store
            var accounts = AccountStore.FindAccountsForService("tasklist");
            if (accounts != null)
            {
                foreach (var acct in accounts)
                {
                    string token;

                    if (acct.Properties.TryGetValue("token", out token))
                    {
                        if (!IsTokenExpired(token))
                        {
                            client.CurrentUser = new MobileServiceUser(acct.Username);
                            client.CurrentUser.MobileServiceAuthenticationToken = token;
                            return;
                        }
                    }
                }
            }

            // Server Flow
            await client.LoginAsync(RootView, "aad");

            // Store the new token within the store
            var account = new Account(client.CurrentUser.UserId);
            account.Properties.Add("token", client.CurrentUser.MobileServiceAuthenticationToken);
            AccountStore.Save(account, "tasklist");
        }

        bool IsTokenExpired(string token)
        {
            // Get just the JWT part of the token (without the signature).
            var jwt = token.Split(new Char[] { '.' })[1];

            // Undo the URL encoding.
            jwt = jwt.Replace('-', '+').Replace('_', '/');
            switch (jwt.Length % 4)
            {
                case 0: break;
                case 2: jwt += "=="; break;
                case 3: jwt += "="; break;
                default:
                    throw new ArgumentException("The token is not a valid Base64 string.");
            }

            // Convert to a JSON String
            var bytes = Convert.FromBase64String(jwt);
            string jsonString = UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            // Parse as JSON object and get the exp field value,
            // which is the expiration date as a JavaScript primative date.
            JObject jsonObj = JObject.Parse(jsonString);
            var exp = Convert.ToDouble(jsonObj["exp"].ToString());

            // Calculate the expiration by adding the exp value (in seconds) to the
            // base date of 1/1/1970.
            DateTime minTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expire = minTime.AddSeconds(exp);
            return (expire < DateTime.UtcNow);
        }
    }
}
```

There are three new pieces to this code.  The first piece is to check to see if there is an existing token in the
KeyStore.  If there is, we check the expiry time and then set up the Azure Mobile Apps client with the username and
token from the KeyStore.  If there isn't, we do the normal authentication process.  If the authentication process is
successful, we reach the second piece, which is to store the token within the KeyStore.  If there is an existing entry,
it will be overwritten.  Finally, there is a method called `IsTokenExpired()` whose only job is to check to see if a
token is expired or not.  This same code can be used in the `Services/iOSLoginProvider.cs`.  The only difference is
in the `AccountStore.Create()` call (as discussed earlier).

I'm using an application specific service ID (or descriptor) for this purpose.  You could also use an identity
provider-based service ID which is especially useful if your mobile client supports multiple identity providers.

Xamarin.Auth only support iOS and Android.  We need to turn to an alternate library for token caching on Universal
Windows.  The standard library has a package called [PasswordVault][33] that can be used identically to the
[KeyStore] and [Keychain] libraries.  Here is the Universal Windows version of the same code in
`Services\UWPLoginProvider.cs`:

```csharp
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.UWP.Services;
using Windows.Security.Credentials;

[assembly: Xamarin.Forms.Dependency(typeof(UWPLoginProvider))]
namespace TaskList.UWP.Services
{
    public class UWPLoginProvider : ILoginProvider
    {
        public PasswordVault PasswordVault { get; private set; }

        public UWPLoginProvider()
        {
            PasswordVault = new PasswordVault();
        }

        public async Task LoginAsync(MobileServiceClient client)
        {
            // Check if the token is available within the password vault
            var acct = PasswordVault.FindAllByResource("tasklist").FirstOrDefault();
            if (acct != null)
            {
                var token = PasswordVault.Retrieve("tasklist", acct.UserName).Password;
                if (token != null && token.Length > 0 && !IsTokenExpired(token))
                {
                    client.CurrentUser = new MobileServiceUser(acct.UserName);
                    client.CurrentUser.MobileServiceAuthenticationToken = token;
                    return;
                }
            }

            // Server-Flow Version
            await client.LoginAsync("aad");

            // Store the token in the password vault
            PasswordVault.Add(new PasswordCredential("tasklist",
                client.CurrentUser.UserId,
                client.CurrentUser.MobileServiceAuthenticationToken));
        }

        bool IsTokenExpired(string token)
        {
            /* Copy code from DroidLoginProvider */
        }
    }
}
```

The PasswordVault replaces the KeyStore (Android) and Keychain (iOS), but the concepts are the same.  All three
mechanisms provide the basic functionality of storing client secrets securely.

## Refresh Tokens

Our token cache checks the token to see if it is expired and prompts the user if the token is no longer valid.  Since
the life of a token is inevitably short (maybe 1 hour), this will still mean that the user is prompted for new
credentials most of the time.  In addition, we have an issue when the app is running for a long time.  What happens
if the user leaves the app running for 2 hours?  The token we received at the start of the session will be invalid
halfway through the session and we will have to restart the app in order to continue.  Both of these situations are
undesirable from the point of view of the user.  Access tokens eventually expire and we need to explicitly deal with
this situation.

The first part of the solution is to request a _Refresh Token_.  This is something the identity provider issues when
the scope of the request includes an offline scope.  Only certain identity providers include the ability to request
refresh tokens.  For server-flow:

* Google: Append the "access_type=offline" to the request.
* Microsoft Account: Select the wl.offline_access scope in the Azure management portal.
* Azure AD: Configure Azure AD to support access to the Graph API.

Facebook and Twitter do not provider refresh tokens.  Once you have the refresh tokens, you can simply call the
refresh API in the Azure Mobile Apps SDK to refresh the token.

> Refresh Tokens are one area that require special consideration when using Custom Authentication.  Just like with
the /.auth/me endpoint, you are on your own when it comes to handling token expiry for custom authentication.

### Configuring Refresh Tokens

You can add the additional information to a Google request with the following code snippet:

```csharp
client.LoginAsync("google", new Dictionary<string, string>
{
    { "access_type", "offline" }  
});
```

Azure Active Directory is perhaps the trickiest to configure.

* Log on to the [Classic Portal][classic-portal].
* Navigate to your Azure Active Directory.
* Go to **APPLICATIONS** and then your WEB application.
* Go to the **CONFIGURE** tab.
* Scroll down to the **Keys** section.

  ![AAD: Add a Key][img59]

* In the **Select duration** drop-down, select _2 Years_.
* Click on **SAVE**.  The key will be generated for you.  Copy the key (you will need it below).
* Go back to the [Azure Portal][portal].
* Go to **App Services**, then your App Service.
* Click on **Tools**, then **Resource explorer**, then **Go**.
* In the Resource Explorer, expand **config** and select **authsettings**.
* Click on **Edit**.
* Set the clientSecret to the key you copied from above.
* Set the additionalLoginParams to `["response_type=code id_token"]`.

  ![AAD: Resource Explorer View][img60]

* Click the **Read/Write** toggle button at the top of the page.
* Click the **PUT** button.

The next time the user logs into our web app side, there will be a one-time prompt to consent to graph API access.
Once granted, the App Service Authentication / Authorization service will start requesting and receiving refresh
tokens.

Once you go through this process and re-authenticate, you will be able to see the refresh token in the output of
the `/.auth/me` endpoint:

![AAD: Refresh Tokens][img61]

Refresh tokens have a different expiry time to the identity token.  The refresh token theoretically lives forever,
but there are "non-use expiry" times. This varies by identity provider.

* Google: 6 months
* Microsoft Account: 24 hours
* Azure Active Directory: 90 days

In addition, there may be other reasons why a token can be invalidated.  For instance, Google provides 25 refresh
tokens per user.  If the user requests more than the limit, the oldest token is invalidated.  You should refer
to the OAuth documentation for the identity provider.

### Using Refresh Tokens

The Azure Mobile Apps Client SDK has a built in method for refreshing tokens for you.  It assumes that you are using
a supported identity provider (Azure Active Directory, Google or Microsoft Account), and have configured the identity
provider to generate the refresh token.



<!-- Images -->
[img59]: img/aad-add-key.PNG
[img60]: img/aad-resource-explorer.PNG
[img61]: img/aad-refresh-token.PNG

<!-- Links -->
[30]: https://components.xamarin.com/gettingstarted/xamarin.auth
[Keychain]: https://developer.apple.com/library/ios/#documentation/security/Reference/keychainservices/Reference/reference.html
[KeyStore]: http://developer.android.com/reference/java/security/KeyStore.html
[33]: https://msdn.microsoft.com/library/windows/apps/windows.security.credentials.passwordvault.aspx
[portal]: https://portal.azure.com/
[classic-portal]: https://manage.windowsazure.com/
