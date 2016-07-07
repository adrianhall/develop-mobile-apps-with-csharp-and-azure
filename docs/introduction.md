# Your First Mobile App

There is a lot of detail to absorb about the possible services that the mobile
client can consume and I will go into significant depth on those subjects.
First, wouldn't it be nice to write some code and get something working?
Microsoft Azure has a great [first-steps tutorial][1] that takes you via the
quickest possible route from creating a mobile backend to having a functional
backend.  I would like to take things a little slower so that we can understand
what is going on while we are doing the process.  We will have practically the
same application at the end.

The application we are going to build together is a simple task list.  The
mobile client will have three screens - an entry screen, a task list and a task
details page.  I have mocked these pages up using [MockingBot][2].

> Mocking your screens before you start coding is a great habit to get into.
There are some great tools available including free tools like [MockingBot][3].
Doing mockups before you start coding is a good way to prevent wasted time later
on.

![Application Mockups for the Task List][img1]

> Why include a back button?  If you are using iOS, then you may want to remove
the back button as the style guides suggest you don't need one.  Other platforms
will need it though, so it's best to start with the least common denominator.
It's the same reason I add a refresh button even though it's only valid on
Windows Phone!

My ideas for this app include:

* Tapping on a task title in the task list will bring up the details page.
* Toggling the completed link in the task list will set the completed flag.
* Tapping the spinner will initiate a network refresh.
* Clicking on Delete Task in the task details will ask "Are you sure?" before deleting the task.

Now that we have our client screens planned out, we can move onto the thinking
about the mobile backend.

## The Mobile Backend

The mobile backend is an ASP.NET WebApi that is served from within Azure App
Service: a highly scalable and redundant web hosting facility that supports all
the major web languages (like ASP.NET, Node, PHP and Python).  

### Creating a Simple Azure Mobile Apps Backend

Microsoft Azure has included a comprehensive starter kit template in the
Azure SDK.  To get started:

1. Fire up Visual Studio 2015
2. Add a new project with File -> New Project...
3. In the **New Project** window:

    - Open up Templates -> Visual C# -> Web and select **ASP.NET Web Application (.NET Framework)**.
    - Enter **Backend** for the Name and **Chapter1** for the Solution name.
    - Pick a suitable directory for the Location field.
    - Click on OK.

    ![New Project Form][img2]

4. In the **New ASP.NET Web Application** window:

    - Click on **Azure Mobile App**.
    - Do **NOT** check "Host in the cloud" or any other checkboxes.
    - Click on OK.

At this point, Visual Studio will create your backend project.

> It's very tempting to select **Azure Mobile Services** instead - it sounds closer to what you want.  Azure Mobile Services is the **OLD** service and is being shut down.  You should not select Azure Mobile Services for any project.

There are a few files of which you should take note.  The Mobile Apps SDK is
initialized within `App_Start\Startup.MobileApp.cs` (with the call to the
configuration routine happening within `Startup.cs`).  The default startup
routine is reasonable but it hides what it is doing behind extension methods.
This technique is fairly common in ASP.NET programs.  Let's expand the configuration
routine to only include what we need:

```csharp
public static void ConfigureMobileApp(IAppBuilder app)
{
    var config = new HttpConfiguration();
    var mobileConfig = new MobileAppConfiguration();

    mobileConfig
        .AddTablesWithEntityFramework()
        .ApplyTo(config);

    Database.SetInitializer(new MobileServiceInitializer());

    app.UseWebApi(config);
}
```

The minimal version of the mobile backend initialization is actually shorter
than the original.  It also only includes a data access layer.  Other services
like authentication, storage and push notifications are not configured.

There is another method in the `App_Start\Startup.MobileApp.cs` file for
seeding data into the database for us.  We can leave that alone for now, but
remember it is there in case we need to adust things for your own backend.

The next important file is the `DbContext` - located in `Models\MobileServiceContext.cs`.
Azure Mobile Apps is heavily dependent on [Entity Framework v6.x][4] and the
`DbContext` is a central part of that library.  Fortunately, we don't need
to do anything to this file right now.  

Finally, we get to the meat of the backend.  The whole point of this demonstration
is to project a single database table - the TodoItem table - into the mobile realm
with the aid of an opinionated [OData v3][5] feed.  To that end, we need three
items:

* A `DbSet<>` within the `DbContext`
* A Data Transfer Object (or DTO)
* A Table Controller

The first item is already taken care of.  However, if we added additional tables,
we would have to modify the `MobileServiceContext`.  The DTO is a special model,
located in the `DataObjects` folder:

```csharp
using Microsoft.Azure.Mobile.Server;

namespace Backend.DataObjects
{
    public class TodoItem : EntityData
    {
        public string Text { get; set; }

        public bool Complete { get; set; }
    }
}
```

Note that the model uses `EntityData` as a base class.  The `EntityData` class
adds five additional properties to the class - we'll discuss those in more
details during the [Data Access and Offline Sync](data.md) chapter.

Finally, let's look at the `Controllers\TodoItemController.cs`:

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

The `TableController` is the central processing for the database access layer.
It handles all the OData capabilities for us.  This means that the actual code
for this controller is tiny - just 12 lines of code.

> You can delete the `Controllers\ValuesController.cs` file if you like - it isn't important for this walkthrough.

We can build the project at this point.  If Visual Studio hasn't done so already,
the missing NuGet packages for Azure Mobile Apps will be downloaded.  There
should not be any errors.  If there are, check the typing for any changes you
made.

### Building an Azure App Service for Mobile Apps

The next step in the process is to build the resources on Azure that will run
your mobile backend.  Start by logging into the [Azure Portal][6], then follow
these instructions:

1. Click on the big **+ New** button in the top-left corner.
2. Click on **Web + Mobile**, then **Mobile App**.
3. Enter a unique name in the **App name** box.

    > Since the name doesn't matter and it has to be unique, you can use [a GUID generator][7] to generate a unique name.  

4. If you have more than one subscription (for example, you have a trial and
    an MSDN subscription), then ensure you select the right subscription in
    the **Subscription** drop-down.
5. Select **Create new** under resource group and enter a name for this mobile application.

    > Resource groups are great for grouping all the resources associated with a mobile application together.  During development, it means you can delete all the resources in one operation.  For production, it means you can see how much the service is costing you and how the resources are being used.

6. Finally, select or create a new **App Service Plan**.

    > The App Service Plan is the thing that actually bills you - not the web or mobile backend.  You can run a number of web or mobile backends on the same App Service Plan.

    I tend to create a new App Service Plan for each mobile application.  This is because the App Service Plan lives inside the Resource Group that you create.  The process is relatively simple.  You have two choices.  The easy choice is where is the service going to run.  In a production environment, the correct choice is "near your customers".  During development "close to the developers" is a good choice.  Unfortunately, neither of those is an option, so you will have to translate into some sort of geographic location.  With 16 regions to choose from, you have a lot of choice.

    The second choice you have to make is what to run the service on - also known as the Pricing tier.  If you click on **View all**, you will see you have lots of choices. F1 Free and D1 Shared, for example, run on shared resources and are CPU limited. You should avoid these as the service will stop responding when you are over the CPU quota.  That leaves Basic, Standard and Premium.  Basic has no automatic scaling and can run up to 3 instances - perfect for development tasks.  Standard and Premium both have automatic scaling and large amounts of storage - they differ in the number of sites or instances you can run on them.  

    Finally, there is a number after the plan - this tells you how big the virtual machine is that the plan is running on.  The numbers differ by number of cores and memory.

    For our purposes, an F1 Free site is enough to run the site unless we run into problems.

7. Once you have created your app service plan and saved it, click on **Create**.

The creation of the service can take a couple of minutes, depending on what else is going on.  Once you have created your app service, the App Service blade will open up.

> What's the difference between a Web App, a Mobile App and an API App?  Not a lot.  The type determines which Quick start projects are available in the Quick start menu under **All settings**.  Since we selected a Mobile app, a set of starter client projects for mobile devices will be presented.

The next step in the process is to create a SQL Azure instance.  The ASP.NET application that we produced earlier will use this to store the data presented in the table controller.

1. Click on the **+ New** button on the left hand side of the page.
2. Select **Data + Storage** and then **SQL Database**.
3. Enter a unique database name (I use a GUID again) in the **Database name** box.
4. Select **Use existing** under the **Resource group**, then select the resource group you created earlier.
5. Select **Blank database** in the **Select source** box.
6. Click on **Configure required settings** for the **Server**.

    - Click on **Create a new server**.
    - Enter another globally unique name for the **Server name** (I use a GUID yet again).
    - Enter **appservice** in the **Server admin login** (or use your own name).
    - Enter a password in the **Password** and **Confirm password** boxes.
    - Select the same location as your App Service in the **Location** box.
    - Click on **Select** to create the Server.

7. Click on the **Pricing tier**.  The **B Basic** plan is the cheapest plan available.
8. Click on **Create**.

The SQL Azure instance takes longer to deploy than the App Service in general.
Unfortunately, you cannot get away without a cost if you are going to develop
mobile backends with C#.

> GUIDs are not the best names to use when you need to actually find resources, but the prevent conflicts when deploying, so I prefer them.  You can prefix them (example: chapter1-GUID) to aid in finding them.  Generally, the first four digits are enough to identify individual resources.

Finally, you will need to link your SQL Azure instance to the App Service instance:

1. Click on **Resource groups** in the left hand side menu.
2. Click on the resource group you created.
3. Click on the App Service you created.
4. Click on **All settings**.
5. Click on **Data connections** in the **MOBILE** menu.
6. Click on **Add**.

    - Click on the **Configure required settings** under **SQL Database**.
    - Select the database you just created, then click on **Select**.
    - Click on **Configure required settings** under **Connection string**.
    - Enter **appservice** in the **User Name** box.
    - Enter your chosen password in the **Password** box.
    - watch for green tick marks to ensure the username and password are correct.
    - Click on **OK**
    - Click on **OK** in the **Add data connection** blade.

This produces another deployment step.  It doesn't take very long so you can
switch back to your Visual Studio window.

> If you want a completely free mobile backend, search for the **Mobile Apps
Quickstart** in the Azure Marketplace.  This template does not require a
database.  It relies on a Node backend, however, so you won't be developing a C#
backend.

### Deploying the Azure Mobile Apps Backend

Deploying to Azure as a developer can be accomplished while entirely within Visual Studio:

1. Right-click on the **Backend** project, then select **Publish...**.
2. Make sure you see this screen shot:

    ![Publish Dialog][img3]

    If you do not see this image, then it is likely you have an older version
    of the Azure SDK installed.  Make sure the Azure SDK version is v2.9 or
    later.

3. Click on **Microsoft Azure App Service**.
4. You may be prompted to enter your Azure credentials here.  Enter the same
    information that you enter to access the Azure Portal.
5. In the lower box, expand the resource group that you created and select the
    app service you created in the portal.
6. Click on **OK**.
7. Click on **Publish**.

Visual Studio will open a browser.  Add `/tables/todoitem?ZUMO-API-VERSION=2.0.0`
to the end of the URL.  This will show the JSON contents of the table that we
defined in the backend.

> You will see the word ZUMO all over the SDK, including in optional HTTP headers and throughout the SDK source code.  ZUMO was the original code name within Microsoft for A**ZU**re **MO**bile.

## The Mobile Client

### Creating a Simple Mobile Client with Xamarin

### Building the Client for Android

### Building the Client for Universal Windows

### Building the Client for iOS

[img1]: img/ch1-pic1.PNG
[img2]: img/ch1-pic2.PNG
[img3]: img/ch1-pic3.PNG

[1]: https://azure.microsoft.com/en-us/documentation/learning-paths/appservice-mobileapps/
[2]: https://mockingbot.com/app/RQe0vlW0Hs8SchvHQ6d2W8995XNe8jK
[3]: https://mockingbot.com/
[4]: https://msdn.microsoft.com/en-us/data/ef
[5]: http://www.odata.org/documentation/odata-version-3-0/
[6]: https://portal.azure.com/
[7]: https://guidgenerator.com/
