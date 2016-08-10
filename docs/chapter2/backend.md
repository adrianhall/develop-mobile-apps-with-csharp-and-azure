# Adding Authentication to a Mobile Backend

The Azure App Service Authentication / Authorization service integrates seamlessly into an Azure Mobile Apps
backend as a piece of middleware that fills in the Identity information for ASP.NET.  That means the only
thing we have to worry about is authorization.  The authentication piece (determining that a user is who they
say they are) is already taken care of.

Authorization (which is the determination of whether an authenticated user can use a specific API) can happen
at either the controller level or an individual operation level.  We can add authorization to an entire table
controller by adding the `[Authorize]` attribute to the table controller.  For example, here is our table
controller from the first chapter with authorization required for all operations:

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

Authorization can also happen on a per-operation basis by adding the `[Authorize]` attribute to a single method
within the table controller.  For example, instead of requiring authorization on the entire table, we want a
version where reading was possible anonymously but updating the database required authentication:

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

Note that the `[Authorize]` attribute can do much more than what is provided here.  Underneath there are various
parameters that you can adjust to see if the user belongs to a specific group or role.  However, the token that
is checked to see if the user is authenticated does not pull in any of the other information that is normally
needed for such authorization tasks.  As a result, the `[Authorize]` tags is really only checking whether a
request requires authentication or not.

# Configuring an Authentication Provider

Configuration of the identity provider is very dependent on the identity provider and whether the client is using
a client-flow or server-flow.  Choose one of the several options for authentication:

* [Enterprise Authentication](enterprise.md) covers Azure Active Directory.
* [Social Authentication](social.md) covers Facebook, Google, Microsoft and Twitter.

We can also configure authentication using custom routes.  This allows us to use other (non-supported) services
or to completely customize our flow (for example, to use an existing identity database).  We will cover custom
authentication later on.
