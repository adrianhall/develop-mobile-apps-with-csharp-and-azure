## Server Side Code

At some point during your mobile client development, you will need to do something a little outside simple
data access.  We have already seen an example of this in custom authentication.  It may be that you want to
kick off a workflow, re-size an image using server-side resources, push a notification to another user, or
do a complex transaction on the database.

Whatever the reason, that time is when you want to use server side code.  The aim is simple enough.  On a
trigger, execute some code and do something with the result.  The trigger can be as simple as a HTTP request
from your client, but could also be in response to a timer, or because something in your environment
happened.  The result can be sent back to the user, placed in storage, or updated in the database.  There
really are no rules when it comes to server side code.

There are, however, options for running server side code.

### Client Processing with WebAPIs and Custom APIs

The first of the set is the venerable [ASP.NET WebAPI][2].  Firstly, configure the ASP.NET application to allow
attribute-based routing.  This is done in your `Startup.MobileApp.cs` file with this line:

```csharp
    config.MapHttpAttributeRoutes();
```

Any WebAPI that you provide must be preceded by a `[Route]` attribute.  We saw an example of this in the
[Custom Authentication][1] section.  In custom authentication, we were setting up an endpoint that allows
us to validate a login request.  The attribute looked like this:

```csharp
namespace Backend.Controllers
{
    [Route(".auth/login/custom")]
    public class CustomAuthController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody] UserInfo body)
        {
            ...
        }
    }
}
```

The ASP.NET WebAPI comes with a bunch of capabilities:

* You can use your regular ASP.NET WebAPI programming techniques.
* You can use Entity Framework for adjusting the database.
* You can use the `[Authorize]` attribute for controlling access.

With ASP.NET WebAPI, you are responsible for absolutely everything.  This option is great for providing
an endpoint to your mobile client that doesn't need anything special.

As flexible as the ASP.NET WebAPI is, most of the time you just want to do something and get the result
returned.  The Custom API feature of the Azure Mobile Apps SDK is a great option because it does a lot
of the scaffolding for you.  With a Custom API:

* Your API appears under '/api' - no exceptions.
* The server enforces the ZUMO-API-VERSION.
* The server emits a X-ZUMO-SERVER-VERSION header in the response.

You can be sure, for example, that a random web crawler is not going to call your Custom API - the web crawler
is not going to provide the `ZUMO-API-VERSION` header, so your code would never be touched.  The Azure
Mobile Apps Client SDK also includes a routine that assumes a Custom API.  For example, let's say you
create a `ValuesController`, then you can call this from your mobile client with the following code:

```csharp
var result = client.InvokeApiAsync<ResultType>('Values');
```

Using `InvokeApiAsync` is a good alternative because it provides the authentication automatically for you
and uses any `DelegatingHandler` classes you have configured.

!!! tip
    You can still use `InvokeApiAsync()` with an ASP.NET WebAPI.

### Background Processing with WebJobs & Azure Functions

The ASP.NET WebAPI and the Custom API provide HTTP endpoints for your mobile clients to interact with.  [WebJobs][3]
and [Azure Functions][4], by comparison, are primed for background tasks.  Things that WebJobs and Azure Functions
are good at:

* Image processing or other CPU-intensive work.
* Queue processing.
* RSS aggregation.
* File and database management.

[WebJobs][3] run in the context of your site.  They use the same set of virtual machines that your website uses and
they share resources with your site.  That means that running memory or CPU intensive jobs can affect your
mobile backend.

[Azure Functions][4] run in a separate project and run in "dynamic compute".  They don't run on your virtual machines.
Rather, they pick up compute power from wherever it is available.  The scaling and lifecycle of the Function
is handled for you by the platform.  The downside is that you have to configure it as a separate project and
you pay for the executions separately.

## Best Practices

For each thing we have defined, we have two choices.  There are reasons to use each and every one of these
options.  So, which do you choose.  Here are my choices:

* Use a [Custom API][i1] for basic web endpoints.
* Use a [WebAPI][2] for custom authentication and anything where you care about the shape of the API.
* Use [WebJobs][i2] for clean-up tasks running on a schedule.
* Use [Functions][i3] for triggered batch processing, like image or queue processing.

Most mobile applications will be able to work with the following:

* A single [WebJob][i2] for cleaning up the database deleted records.
* A [Custom API][i1] for doing transaction work or triggering a batch process.
* An [Azure Function][i3] for doing the batch processing.

<!-- URLs -->
[i1]: custom.md
[i2]: webjobs.md
[i3]: functions.md
[1]: ../chapter2/custom.md
[2]: https://www.asp.net/web-api
[3]: https://azure.microsoft.com/en-us/documentation/articles/websites-webjobs-resources/
[4]: https://azure.microsoft.com/en-us/documentation/articles/functions-overview/
