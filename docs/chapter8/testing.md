# Testing your Mobile Application

There is nothing that causes more problems than when a developer works on testing.  Testing a cross-platform client-server application across all the permutations that are possible is hard work.  You will spend more time on developing tests than on writing code.  Much of what is asked, however, is not required.  That is primarily because most people want to test the entire stack.  There are generally minimal custom code in the backend, so that can significantly reduce the amount of tests you write.

In this section, we will look at what it takes to do unit tests for your mobile backend and the mobile app, together with an end-to-end testing capability that allows you to test your application on many devices at once.

## Testing your Mobile Backend

Most of the code within the mobile backend is pulled from libraries - ASP.NET, Entity Framework and Azure Mobile Apps.  These libraries are already tested before release and there is not much you can do about bugs other than reporting them (although Azure Mobile Apps does accept fixes as well).  As a result, you should concentrate your testing on the following areas:

*  Filters, Transforms and Actions associated with your table controllers.
*  Custom APIs.

In addition, your mobile backend will come under a lot of strain after you go to production.  You should plan on a load test prior to each major release in a staging environment that is identical to your production environment.  Never do a load test on your production environment after you have users, as such testing will affect your users.

### Unit Testing

Let's take a simple example of an app that we developed back in Chapter 3.  We used the data connections to develop a personal todo store - one in which the users ID is associated with each submitted record and the user could only see their own records.  The table controller looked like the following:

```csharp
namespace Backend.Controllers
{
    public class TodoItemController : TableController<TodoItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request, enableSoftDelete: true);
        }

        public string UserId => ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier).Value;

        public void ValidateOwner(string id)
        {
            var result = Lookup(id).Queryable.PerUserFilter(UserId).FirstOrDefault<TodoItem>();
            if (result == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        // GET tables/TodoItem
        public IQueryable<TodoItem> GetAllTodoItems()
        {
            return Query().PerUserFilter(UserId);
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TodoItem> GetTodoItem(string id)
        {
            return new SingleResult<TodoItem>(Lookup(id).Queryable.PerUserFilter(UserId));
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch)
        {
            ValidateOwner(id);
            return UpdateAsync(id, patch);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            item.UserId = UserId;
            TodoItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTodoItem(string id)
        {
            ValidateOwner(id);
            return DeleteAsync(id);
        }
    }
}
```

In addition, we have a LINQ extension method for handling the `PerUserFilter`:

```csharp
using Backend.DataObjects;
using System.Linq;

namespace Backend.Extensions
{
    public static class PerUserFilterExtension
    {
        public static IQueryable<TodoItem> PerUserFilter(this IQueryable<TodoItem> query, string userid)
        {
            return query.Where(item => item.UserId.Equals(userid));

        }
    }
}
```

In my minimalist testing suggestion, I would test the following:

* The LINQ Extension `PerUserFilter`.
* The `UserId` property.
* The `ValidateOwner` method.

The other methods are straight out of the standard table controller.  I would defer unit testing of these until the end-to-end tests.  Unit tests should be short and should be idempotent.  The test should be able to be run multiple times and always return the same result.  Since our service is defined to be run out of a stateful SQL database, it cannot be defined to be idempotent.  However, the individual parts we are operating can be idempotent.

Unit tests are generally defined to be a separate project within the Visual Studio solution.  By convention, they are named by appending `.Tests` to the project they are testing.  My project is called `Backend`, so the test project is called `Backend.Tests`.  To create the test project:

*  Open the solution in Visual Studio.
*  Right-click the solution, choose **Add** -> **New Project...**.
*  Select **Installed** > **Visual C#** > **Test** in the project type tree.
*  Select **xUnit Test Project** as the project type.
*  Enter **Backend.Tests** as the name, then click **OK**. 

!!! info "xUnit vs. MSTest"
    Most version of Visual Studio support a specific type of test called [MSTest][1].  However, Visual Studio 2017 has integrated [xUnit][2] testing as well.  xUnit is cross-platform whereas MSTest is PC only.  I will be using xUnit for this project.  If you are using a version of Visual Studio earlier than VS2017, you will not have the xUnit Test Project available.  However, you can [simulate the same project type manually][3].

Generally, copy the folder format from the main project to the test project.  For example, the `PerUserFilterExtension.cs` file is in an `Extensions` folder within the main project.  I'm going to create an `Extensions` folder within the test project and create a `PerUserFilterExtensionTests.cs` file with the tests in it.  To create the tests:

*  Right-click the `Extensions` folder, and select **Add** -> **New Item...**.
*  Select **Installed** > **Visual C# Items** > **Test** in the project type tree.
*  Select **xUnit Test**, and enter `PerUserFilterExtensionTests.cs` as the name.
*  Click **Add**.

### Load Testing

## Testing your Mobile Client

### Introduction to Mobile Client Testing

### Using Mock Data Services

### Unit Testing



## End to End Testing
### Introduction to Xamarin Test Cloud

<!-- Links -->
[1]: https://msdn.microsoft.com/en-us/library/hh694602.aspx
[2]: https://xunit.github.io/
[3]: http://xunit.github.io/docs/getting-started-desktop.html
