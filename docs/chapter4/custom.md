# Custom HTTP Endpoints

Azure Mobile Apps makes it really easy to develop basic APIs that can be used in mobile clients.  Most
custom APIs can be simply invoked.  Azure Mobile Apps takes care of most of the scaffolding for you. The
server SDK will:

*  Ensure the `ZUMO-API-VERSION` is present and valid.
*  Handle serialization and deserialization of JSON.
*  Ensure the API is given an appropriate URL.

All the custom APIs will appear under the `/api` endpoint.  For example, if you created a controller
called `FooController`, it would be invoked by sending messages to `/api/Foo`.  This is case-insensitive,
so you could also reference this API as `/api/foo`.

### Configuring Custom APIs

Before anything happens, you must add the `MapApiControllers()` method to the `MobileAppConfiguration()`
call.  This is done in the `ConfigureMobileApp()` method in `App_Start\Startup.MobileApp.cs` file:

```csharp
    new MobileAppConfiguration()
        .AddTablesWithEntityFramework()     /* /tables endpoints */
        .MapApiControllers()                /* /api endpoints */
        .ApplyTo(config);
```

The `MapApiControllers()` extension method does the actual work of looking for custom APIs and mapping them
onto the `/api` endpoint.

### Creating a Basic Custom API

You might remember that the original Azure Mobile Apps project within Visual Studio comes with a sample
custom API called the `ValuesController`.  This controller did not do anything useful.  Let's re-create
it from scratch.

*  Right-click the `Controllers` node in your backend project and use **Add** -> **Controller...**.

   ![][img1]

*  Select the **Azure Mobile Apps Custom Controller**, then click **Add**.
*  Enter the name for the controller, for example, **ValuesController**.  Click **Add**.

The new controller will be scaffolded for you.  When you are done, it looks like this:

```csharp
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Backend.Controllers
{
    [MobileAppController]
    public class ValuesController : ApiController
    {
        // GET api/Default
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
```

If you have not done anything that requires a backend, then you can press F5 to run the backend and use
Postman to interact with your new custom API:

![][img2]

We still have to submit the `ZUMO-API-VERSION` header for this to work.  Whatever my method returns will
be returned as JSON.  This one is not exactly exciting.  One of the things I do quite often is provide
a configuration endpoint called `/api/config` which returns a JSON object that I can use to configure the
mobile client.

```csharp
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Collections.Generic;
using System;

namespace Backend.Controllers
{
    [MobileAppController]
    public class ConfigController : ApiController
    {
        private ConfigViewModel configuration;

        public ConfigController()
        {
            Dictionary<string, ProviderInformation> providers = new Dictionary<string, ProviderInformation>();

            AddToProviders(providers, "aad", "MOBILE_AAD_CLIENT_ID");
            AddToProviders(providers, "facebook", "MOBILE_FB_CLIENT_ID");
            AddToProviders(providers, "google", "MOBILE_GOOGLE_CLIENT_ID");
            AddToProviders(providers, "microsoftaccount", "MOBILE_MSA_CLIENT_ID");
            AddToProviders(providers, "twitter", "MOBILE_TWITTER_CLIENT_ID");

            configuration = new ConfigViewModel
            {
                AuthProviders = providers
            };
        }

        private void AddToProviders(Dictionary<string, ProviderInformation> providers, string provider, string envVar)
        {
            string envVal = Environment.GetEnvironmentVariable(envVar);
            if (envVal != null && envVal?.Length > 0)
            {
                providers.Add(provider, new ProviderInformation { ClientId = envVal });
            }

        }

        [HttpGet]
        public ConfigViewModel Get()
        {
            return configuration;
        }
    }

    public class ProviderInformation
    {
        public string ClientId { get; set; }
    }

    public class ConfigViewModel
    {
        public Dictionary<string, ProviderInformation> AuthProviders { get; set; }
    }
}
```

The constructor produces a `ConfigViewModel` for me.  This describes the configuration object I want to send.  In
this case, I want to send the client ID for each authentication provider.  If the authentication provider is not
configured, then the client ID is not sent.  I use the application settings to determine what is configured. The
primary idea behind this is to integrate all the client flows within my mobile client.  When the user wishes to
log in, they are presented with a menu of options and can pick which social provider they wish to use.  The
client-flow authentication libraries may use different client IDs than the ones that are configured into the authentication
service.  For example, AAD uses two client IDs - one for server-flow and one for client-flow.  As a result, this
controller uses **Application Settings** (which appear as environment variables to the backend) to set the
client IDs.  The result of calling this API from Postman looks like this:

![][img3]

!!! warn
    Only expose information that you would normally and reasonably embed in a mobile client.  Never transmit
    secrets this way.  It is insecure and can put your entire authentication system at risk of hijack.

You can read this information using the Azure Mobile Apps Client SDK once you have a client reference, using
the same model classes:

```csharp
var configuration = await client.InvokeAsync<ConfigViewModel>("config", HttpMethod.Get, null);
```

You must specify a class that deserializes the JSON that is produced by your API.  If you use the same classes,
that is practically guaranteed.  The other methods in call are the HTTP Method (GET, POST, PATCH, DELETE, etc.)
and the query parameters.

### Handling Parameters to a Custom API

The `/api/config` endpoint didn't require any information that is extra.  Sometimes, we need to provide
extra information so that the right thing can be produced.  For example, consider the case of uploading
or downloading a file to Azure Storage.  We may want to provide some extra information - the filename of
the file we want to upload and the permissions for the file.  Uploading and downloading files is discussed
more fully [later][2] in the book and offers a fuller example of this concept.

To illustrate the concept clearly, let's create an API that adds two numbers together.  We would call
this API through HTTP like this: `GET /api/addition?first=1&second=2`.  The first number gets added to
the second number and we will return the result.  If the first or the second number doesn't exist, we
want to produce a 400 Bad Request response rather than crashing the server.  Here is the code:

```csharp
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Net;

namespace Backend.Controllers
{
    [MobileAppController]
    public class AdditionController : ApiController
    {
        // GET api/Addition
        public ResultViewModel Get(int? first, int? second)
        {
            if (first == null || second == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            ResultViewModel results = new ResultViewModel
            {
                First = first.GetValueOrDefault(),
                Second = second.GetValueOrDefault()
            };
            results.Result = results.First + results.Second;
            return results;
        }
    }

    public class ResultViewModel
    {
        public int First { get; set; }
        public int Second { get; set; }
        public int Result { get; set; }
    }
}
```

If you try this out, you will notice something rather odd.  Try doing the following URL: `/api/addition?first=1&second=2`.
You will note it works as expected.  However, if you try doing the following URL: `/api/addition?first=1`, then you
will note that you get a **404 Not Found**.  This makes the API easy to write because you don't have to worry about
your code receiving bad input (most of the time).  However, you may not get the API surface that you want.  In this
case, I want to return a **400 Bad Request** instead of the normal 404 response.  I have to do a lot more work to
support this case:

```csharp
    public class AdditionController : ApiController
    {
        // GET api/Addition
        public ResultViewModel Get()
        {
            int? first = GetParameter(Request, "first"),
                 second = GetParameter(Request, "second");

            ResultViewModel results = new ResultViewModel
            {
                First = first.GetValueOrDefault(),
                Second = second.GetValueOrDefault()
            };
            results.Result = results.First + results.Second;
            return results;
        }

        private int? GetParameter(HttpRequestMessage request, string name)
        {
            var queryParams = request.GetQueryNameValuePairs().Where(kv => kv.Key == name).ToList();
            if (queryParams.Count == 0)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            int rv;
            if (!Int32.TryParse(queryParams[0].Value, out rv))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            return rv;
        }
    }
```

When our Get() routine does not take parameters, no particular pattern is required.  We can use the `Request`
object to access the parameters we send.  In this case, the `GetParameter()` routine checks to see if there
is a named parameter and converts it to an integer.  If the named parameter is not there or it is not numeric,
then a Bad Request response is sent.

### Handling POST Requests

GET and DELETE requests take parameters on the URI.  These cam ne dealt with via the automatic conversion to
method parameters or they can be handled via LINQ queries on the request object, as we observed in the prior
section.  POST requests, by contrast, allow you to submit a JSON body for processing.  This is useful when we
want to submit multiple JSON objects for processing.  For example, one of the common requirements we have is
for transactions.


<!-- Images -->
[img1]: img/add-custom-controller-1.PNG
[img2]: img/add-custom-controller-2.PNG
[img3]: img/add-custom-controller-3.PNG

<!-- URLs -->
[1]: ../chapter2/custom.md
[2]: ../chapter6/concepts.md
