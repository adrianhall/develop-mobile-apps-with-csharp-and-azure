# Claims and Authorization

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
can't use roles either.  Clearly, we are going to have to do something else.

## Obtaining User Claims

At some point you are going to need to deal with something other than the claims that are
in the token passed for authentication.  One of those times is during Authorization.  We
are going to set up an authorization based on claims. Fortunately, the Authentication /
Authorization feature has an endpoint for that at `/.auth/me`:

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

The same `/.auth/me` endpoint is available in code (much simpler) on the server.   To get the extra
information, we need to query the `User` object:

```csharp
var identity = await User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(Request);
```

There is one `Credentials` class for each supported authentication technique - Azure Active Directory, Facebook,
Google, Microsoft Account and Twitter.  These are in the **Microsoft.Azure.Mobile.Server.Authentication**
namespace.  They all follow the same pattern as the model we created for the client - there are Provider,
UserId and UserClaims properties.  The token and any special information will be automatically decoded
for you.  For instance, the TenantId is pulled out of the response for Azure AD.  The UserClaims property
is the one we are interested in - it is an enumeration of the claims that the Authentication /
Authorization system received from the identity provider.

> You can use the AccessToken property to do Graph API lookups for most providers in a custom API.
We'll get into this more in a later Chapter.

In its simplest form, you can query the user claims to determine if the user is authorized after using
the `[Authorize]` attribute.

> If you haven't done so already, set up your client and server for Azure AD client flow and [add the
security groups to the claims](./enterprise.md#addlclaims).


[img55]: img/user-identity.PNG
[img56]: img/user-claims.PNG
[img57]: img/auth-me.PNG