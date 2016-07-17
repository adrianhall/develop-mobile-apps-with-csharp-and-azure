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
remember it is there in case you need to seed data into a new database for
your own backend.

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
details during the [Data Access and Offline Sync][int-data] chapter.

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

    I tend to create a new App Service Plan for each mobile application.  This is because the App Service Plan lives inside the Resource Group that you create.  The process is relatively simple.  You have two decisions to make.  The first decision is where is the service going to run.  In a production environment, the correct choice is "near your customers".  "Close to the developers" is a good choice during development.  Unfortunately, neither
    of those is an option, so you will have to translate into some sort of geographic location.  With 16 regions to choose from, you have a lot of choice.

    The second decision you have to make is what to run the service on; also known as the Pricing tier.   If you click on **View all**, you will see you have lots of choices. F1 Free and D1 Shared, for example, run on shared resources and are CPU limited. You should avoid these as the service will stop responding when you are over the CPU quota.  That leaves Basic, Standard and Premium.  Basic has no automatic scaling and can run up to 3 instances - perfect for development tasks.  Standard and Premium both have automatic scaling and large amounts of storage; they differ in features: the number of sites or instances you can run on them, for example.  Finally, there is a number after the plan.  This tells you how big the virtual machine is that the plan is running on.  The numbers differ by number of cores and memory.

    For our purposes, an F1 Free site is enough to run this small demonstration project.

7. Once you have created your app service plan and saved it, click on **Create**.

The creation of the service can take a couple of minutes.  Once you have created your app service, the App Service blade will open.

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

> There are other methods of creating a SQL Azure instance, including using
the Data Connections blade within the App Service.

The SQL Azure instance takes longer to deploy than the App Service in general.
However, it will still be available within 3-5 minutes.

> GUIDs are not the best names to use when you need to actually find resources, but using GUIDS prevents conflicts when deploying, so I prefer them as a naming scheme.  You can prefix the GUID (example: chapter1-GUID) to aid in discovery later on.  Generally, the first four digits of a GUID are enough to identify individual resources.

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

> You will see the word ZUMO all over the SDK, including in optional HTTP headers and throughout the SDK source code.  ZUMO was the original code name within Microsoft for A<b>ZU</b>re <b>MO</b>bile.

## The Mobile Client

Now that the mobile backend is created and deployed, we can move onto the client
side of things.  First of all, let's prepare the Visual Studio instance.  If you
have installed the Cross-Platform Mobile tools during the installation, most of
the work has already been done.  However, you may want to install the [Xamarin
Forms Templates][8] using the Tools -> Extensions and Updates...

  ![Installing the Xamarin Forms Templates][img4]

This template pack provides additional templates for Xamarin Forms development
that I find useful.  Most notably, there is a specific template for a mobile
cross-platform project covering the Android, iOS and UWP mobile platforms.  

### Creating a Simple Mobile Client with Xamarin

Now that we have prepared your Visual Studio instance, we can create the project.
Right-click on the solution and select **Add** -> **New Project...**.  This will
bring up the familiar New Project dialog.  The project you want is under **Visual C#**
-> **Cross-Platform**, and is called **Xamarin.Forms (UWP/Android/iOS)**.  If you
did not install the Xamarin Forms Template add-on, then choose the
**Blank Xaml App (Xamarin.Forms Portable)** project.  Give the project a name,
then click on **OK**.

  ![Creating the Xamarin Forms Project][img5]

Project creation will take longer than you expect, but there is a lot going on.
If you have never created a mobile or UWP project before, you will be prompted
to turn on Windows 10 Developer Mode:

  ![Turn on Developer Mode][img6]

Developer mode in Windows 10 allows you to run unsigned binaries for development
purposes and to turn on debugging so that you can step through your UWP programs
within Visual Studio.

We will also get asked to choose what version of the Universal Windows platform
we want to target:

  ![UWP Platform Chooser][img7]

Version 10240 was the first version of Windows 10 that was released to the general
public, so that's a good minimum version to pick.  In general, the defaults for
the Universal Windows Platform choice are good enough.

Finally, we will be asked about our iOS build host.  This must be some sort of
mac.  As I said previously, I use a Mac Mini underneath my desk for this. The
latest Xamarin tools forego a dedicated build service and instead use a secure
shell (ssh) connection to connect to the Mac.  That means you must go through
the process for [setting up the mac for ssh access][9].  

When prompted about the Xamarin Mac Agent, click on **OK** to get the list of
local mac agents:

  ![Xamarin Mac Agent - Host List][img8]

Highlight your mac (in case there are multiples), then click on **Connect...**.
You will be prompted for your username and password:

  ![Xamarin Mac Agent - Login][img9]

Just enter the username and password that you use to log in to your mac and click
on **Login**.

> **What's my username?**  Apple tries very hard to hide the real username of
your account from you.  The easiest way to find your mac username is to open up
the Finder.  The name next to your home icon is the name of your account.

Once the project is created, you will see that four new projects have been
created: a common library which you named plus one project for each platform
that has been chosen.  Since we chose a project with three platforms, we get
four projects:

  ![The TaskList Project Layout][img10]

Most of our work will happen in the common library.  However, we can introduce
platform-specific code at any point.  The platform-specific code is stored in
the platform-specific project.

There is one final item we must do before we leave the set up of the project.
There are a number of platform upgrades that inevitably have to happen.  The
Xamarin Platform is updated much more often than the Visual Studio plugin - the
updates are released via NuGet: the standard method of distributing libraries
for .NET applications.  In addition to the inevitable Xamarin Platform update,
we also will want to add the following NuGet packages:

*  Microsoft.Azure.Mobile.Client v2.0.0 or later
*  Newtonsoft.Json v6.0.3 or later

> Although it is tempting, do not include a v1.x version of the Mobile Client.
This is for the earlier Azure Mobile Services.  There are many differences between
the wire protocols of the two products.

You can install the NuGet packages by right-clicking on the project and selecting
**Manage NuGet Packages...**.

  ![Manage NuGet Packages][img11]

You must install the updates and the new NuGet packages on all four projects.  
This involves repeating the same process for each client project in your
solution.

> Android generally has more updates than the other platforms.  Ensure that you
update the main Xamarin.Forms package and then refresh the update list.  This will
ensure the right list of packages is updated.

### Building the Common Library

There are two parts that we must concentrate on within the common library.  The
first is the connection to Azure Mobile Apps and the second is in the pages
that the user interacts with.  In both cases, there are best practices to observe.

#### Building an Azure Mobile Apps Connection

We will rely on interfaces for defining the shape for the class for any service
that we interact with.  This is really not important in small projects like this
one.  This technique allows us to mock the backend service, as we shall see
later on.  Mocking the backend service is a great technique to rapidly iterate
on the front end mobile client without getting tied into what the backend is doing.

Let's start with the cloud service - this is defined in `Abstractions\ICloudService.cs`.
It is basically used for initializing the connection and getting a table definition:

```csharp
namespace TaskList.Abstractions
{
    public interface ICloudService
    {
        public ICloudTable<T> GetTable<T>() where T : TableData;
    }
}
```

There is a dependent implementation here: the `ICloudTable` generic interface.  This
represents a CRUD interface into our tables and is defined in `Abstractions\ICloudTable.cs`:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskList.Abstractions
{
    public interface ICloudTable<T> where T : TableData
    {
        Task<T> CreateItemAsync(T item);
        Task<T> ReadItemAsync(string id);
        Task<T> UpdateItemAsync(T item);
        Task DeleteItemAsync(T item);

        Task<ICollection<T>> ReadAllItemsAsync() where T : TableData;
    }
}
```

The `ICloudTable<T>` interface defines the normal CRUD operations: Create, Read,
Update and Delete.  However, it does so asynchronously.  We are dealing with network
operations in general so it is easy for those operations to tie up the UI thread
for an appreciable amount of time.  Making them async provides the ability to
respond to other events.  I also provide a `ReadAllItemsAsync()` method that
returns a collection of all the items.

There are some fields that every single record within an Azure Mobile Apps table
provides.  These fields are required for offline sync capabilities like incremental
sync and conflict resolution.  The fields are provided by an abstract base class
on the client called `TableData`:

```csharp
using System;

namespace TaskList.Abstractions
{
    public abstract class TableData
    {
        public string Id { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public byte[] Version { get; set; }
    }
}
```

As we will learn when we deal with table data in Chapter 3, these fields need to
be defined with the same name and semantics as on the server.  Our model on
the server was sub-classed from `EntityData` and the `EntityData` class on the
server defines these fields.

It's tempting to call the client version of the class the same as the server
version.  If we did that, the models on both the client and server would look
the same.  However, I find that this confuses the issue.  The models on the
client and server are not the same.  They are missing the `Deleted` flag and
they do not contain any relationship information on the client.  I choose to
deliberately call the base class something else on the client to avoid this
confusion.

We will be adding to these interfaces in future chapters as we add more
capabilities to the application.

The concrete implementations of these classes are similarly easily defined.  The
Azure Mobile Apps Client SDK does most of the work for us.  Here is the concrete
implementation of the `ICloudService` (in `Services\AzureCloudService.cs`):

```csharp
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;

namespace TaskList.Services
{
    public class AzureCloudService : ICloudService
    {
        MobileServiceClient client;

        public AzureCloudService()
        {
            client = new MobileServiceClient("https://my-backend.azurewebsites.net");
        }

        public ICloudTable<T> GetTable<T>() where T : TableData
        {
            return new AzureCloudTable<T>(client);
        }
    }
}
```

The Azure Mobile Apps Client SDK takes a lot of the pain out of communicating
with the mobile backend that we have already published.  Just swap out the
name of your mobile backend and the rest is silently dealt with.  

> The name `Microsoft.WindowsAzure.MobileServices` is a hold-over from the old Azure Mobile Services code-base.  Don't be fooled - clients for Azure Mobile Services are not interchangeable with clients for Azure Mobile Apps.  

We also need a concrete implementation of the `ICloudTable<T>` interface (in `Services\AzureCloudTable.cs`):

```csharp
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Abstractions;

namespace TaskList.Services
{
    public class AzureCloudTable<T> : ICloudTable<T> where T : TableData
    {
        MobileServiceClient client;
        IMobileServiceTable<T> table;

        public AzureCloudTable(MobileServiceClient client)
        {
            this.client = client;
            this.table = client.GetTable<T>();
        }

        #region ICloudTable implementation
        public async Task<T> CreateItemAsync(T item)
        {
            await table.InsertAsync(item);
            return item;
        }

        public async Task<T> DeleteItemAsync(T item)
        {
            await table.DeleteAsync(item);
        }

        public async Task<ICollection<T>> ReadAllItemsAsync()
        {
            return await table.ToListAsync();
        }

        public async Task<T> ReadItemAsync(string id)
        {
            return await table.LookupAsync(id);
        }

        public async Task<T> UpdateItemAsync(T item)
        {
          await table.UpdateAsync(item);
          return item;
        }
        #endregion
    }
}
```

It's important to note here that the Azure Mobile Apps Client SDK does a lot of
the work for us.  In fact, we are just wrapping the basic interface here.  This
won't normally be the case, but you can see that the majority of the code for
dealing with the remote server is done for us.

> You can use a shorthand (called a lambda expression) for methods with only one line.  For instance, the delete method could just as easily have been written as `public async Task<T> DeleteItemAsync(T item) => await table.DeleteAsync(item);`.  You may see this sort of short hand in samples.

We also need to create the model that we will use for the data.  This should
look very similar to the model on the server - including having the same name
and fields.  In this case, it's `Models\TodoItem.cs`:

```csharp
using TaskList.Abstractions

namespace TaskList.Models
{
    public class TodoItem : TableData
    {
        public string Text { get; set; }
        public bool Complete { get; set; }
    }
}
```

We have a final piece of code to write before we move on to the views, but it's
an important piece.  The `ICloudService` must be a singleton in the client.  We
will add authentication and offline sync capabilities in future versions of this
code.  The singleton becomes critical when using those features.  For right now,
it's good practice and saves on memory if you only have one copy of the `ICloudService`
in your mobile client.  Since there is only one copy of the `App.cs` in any
given app, I can place it there.  Ideally, I'd use some sort of dependency
injection system or a singleton manager to deal with this.  Here is the `App.cs`:

```csharp
using TaskList.Abstractions;
using TaskList.Services;
using Xamarin.Forms;

namespace TaskList
{
    public class App : Application
    {
        public static ICloudService CloudService { get; set; }

        public App()
        {
            CloudService = new AzureCloudService();
            MainPage = new NavigationPage(new Pages.EntryPage());
        }

        // There are lifecycle methods here...
    }
}
```

We haven't written `Pages.EntryPage` yet, but that's coming.  The original `App.cs`
class file had several methods for handling lifecycle events like starting, suspending
or resuming the app.  I did not touch those methods for this example.

#### Building the UI for the App

Earlier, I showed the mockup for my UI.  It included three pages - an entry
page, a list page and a detail page.  These pages have three elements - a
XAML definition file, a (simple) code-behind file and a view model.

> This book is not intending to introduce you to everything that there is to know about Xamarin and UI programming with XAML.  If you wish to have that sort of introduction, then I recommend reading the excellent book by Charles Petzold: [Creating Mobile Apps with Xamarin.Forms][10].

I tend to use MVVM (or Model-View-ViewModel) for UI development in Xamarin
based applications.  It's a nice clean pattern and is well understood and
documented.  In MVVM, there is a 1:1 correlation between the view and the
view-model, 2-way communication between the view and the view-model and
properties within the view-model are bound directly to UI elements.  In
general (and in all my code), view-models expose an INotifyPropertyChanged
event to tell the UI that something within the view-model has been changed.

To do this, we will use a `BaseViewModel` class that implements the base functionality
for each view.  Aside from the `INotifyPropertyChanged` interface, there are
some common properties we need for each page.  Each page needs a title, for
example, and each page needs an indicator of network activity.  These can be
placed in the `Abstractions\BaseViewModel.cs` class:

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TaskList.Abstractions
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        string _propTitle = string.Empty;
        bool _propIsBusy;

        public string Title
        {
            get { return _propTitle; }
            set { SetProperty(ref _propTitle, value, "Title"); }
        }

        public bool IsBusy
        {
            get { return _propIsBusy; }
            set { SetProperty(ref _propIsBusy, value, "IsBusy"); }
        }

        protected void SetProperty<T>(ref T store, T value, string propName, Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(store, value))
                return;
            store = value;
            if (onChanged != null)
                onChanged();
            OnPropertyChanged(propName);
        }

        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged == null)
                return;
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
```

This is a fairly common `INotifyPropertyChanged` interface implementation pattern.
Each property that we want to expose is a standard property, but the `set` operation
is replaced by the `SetProperty()` call.  The `SetProperty()` call deals with the
notification; calling the event emitter if the property has changed value.  We
only need two properties on the `BaseViewModel`: the title and the network indicator.

I tend to write my apps in two stages.  I concentrate on the functionality of the
app in the first stage.  There is no fancy graphics, custom UI widgets, or anything
else to clutter the thinking.   The page is all about the functionality of the
various interactions.  Once I have the functionality working, I work on the styling
of the page.  We won't be doing any styling work in the demonstration apps that we
write during the course of this book.

The EntryPage has just one thing to do.  It provides a button that enters the app.
When we cover authentication later on, we'll use this to log in to the backend.  If
you are looking at the perfect app, this is a great place to put the introductory
screen.

Creating a XAML file is relatively simple.  First, create a `Pages` directory to
hold the pages of our application.  Then right-click on the `Pages` directory in
the solution explorer and choose **Add** -> **New Item...**.  In the **Add New Item**
dialog, pick **Visual C#** -> **Cross-Platform** -> **Forms Xaml Page**.  Name the
new page `EntryPage.cs`.  This will create two files - `EntryPage.xaml` and
`EntryPage.xaml.cs`.  Let's center a button on the page and wire it up with
a command.  Here is the `Pages\EntryPage.xaml` file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TaskList.Pages.EntryPage"
             Title="{Binding Title}">
    <ContentPage.Content>
        <StackLayout HorizontalOptions="Center"
                     Orientation="Vertical"
                     VerticalOptions="Center">
            <Button BackgroundColor="Teal"
                    BorderRadius="10"
                    Command="{Binding LoginCommand}"
                    Text="Login"
                    TextColor="White" />
        </StackLayout>
    </ContentPage.Content>
</ContentPage>
```

There are a couple of interesting things to note here.  The `StackLayout` element
is our layout element.  It occupies the entire screen (since it is a direct child
of the content page) and the options just center whatever the contents are.  The
only contents are a button.

There are two bindings.  These are bound from the view-model.  We've already screen
the Title property - this is a text field that specifies the title of the page.
The other binding is a login command.  When the button is tapped, the login command
will be run.  We'll get onto that in the view-model later.

The other part of the XAML is the code-behind file.  Because we are moving all
of the non-UI code into a view-model, the code-behind file is trivial:

```csharp
using TaskList.ViewModels;
using Xamarin.Forms;

namespace TaskList.Pages
{
    public partial class EntryPage : ContentPage
    {
        public EntryPage()
        {
            InitializeComponent();
            BindingContext = new EntryPageViewModel();
        }
    }
}
```

This is a recipe that will be repeated over and over again for the code-behind
when you are using a XAML-based project with MVVM.  We initialize the UI, then
bind all the bindings to a new instantiation of the view model.  

Talking of which, the view-model needs just to handle the login click.  Note that
the location or namespace is `TaskList.ViewModels`.  I'm of two minds about location.
There tends to be a 1:1 relationship between the XAML file and the View Model, so
it makes sense that they are stored together.  However, just about all the sample
code that I see has the view-models in a separate namespace.  Which one is correct?
I'll go with copying the samples for now.  Here is the code for `ViewModels\EntryPageViewModel.cs`:

```csharp
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TaskList.Abstractions;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class EntryPageViewModel : BaseViewModel
    {
        public EntryPageViewModel()
        {
            Title = "Task List";
        }

        Command loginCmd;
        public Command LoginCommand => loginCmd ?? (loginCmd = new Command(async () => await ExecuteLoginCommand()));

        async Task ExecuteLoginCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                Application.Current.MainPage = new NavigationPage(new Pages.TaskList());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login] Error = {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
```

This is a fairly simple view-model but there are some patterns here that are
worth explaining.  Firstly, note the way we create the `LoginCommand` property.
This is the property that is bound to the `Command` parameter in the `Button`
of our view.  This recipe is the method of invoking a UI action asynchronously.
It isn't important now, but we will want this technique repeatedly as our UI
actions kick off network activity.

The second is the pattern for the `ExecuteLoginCommand` method.  Firstly, I
ensure nothing else is happening by checking the IsBusy flag.   If nothing
is happening, I set the IsBusy flag.  Then I do what I need to do in a try/catch
block.  If an exception is thrown, I deal with it.  Most of the time this
involves displaying an error condition.  There are several cross-platform dialog
packages to choose from or you can roll your own.  That is not covered here.  We
just write a debug log statement so we can see the result in the debug log.  Once
everything is done, we clear the IsBusy flag.

The only thing we are doing now is swapping out our main page for a new main
page.  This is where we will attach authentication later on.

The next page is the Task List page, which is in `Pages\TaskList.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TaskList.Pages.TaskList"
             Title="{Binding Title}">
  <ContentPage.Content>
    <StackLayout>
      <ListView BackgroundColor="#7F7F7F"
                CachingStrategy="RecycleElement"
                IsPullToRefreshEnabled="True"
                IsRefreshing="{Binding IsBusy, Mode=OneWay}"
                ItemsSource="{Binding Items}"
                RefreshCommand="{Binding RefreshCommand}"
                RowHeight="50"
                SelectedItem="{Binding SelectedItem, Mode=TwoWay}">
        <ListView.ItemTemplate>
          <DataTemplate>
            <ViewCell>
              <StackLayout HorizontalOptions="FillAndExpand"
                           Orientation="Horizontal"
                           Padding="10"
                           VerticalOptions="CenterAndExpand">
                <Label HorizontalOptions="FillAndExpand"
                       Text="{Binding Text}"
                       TextColor="#272832" />
                <Switch IsToggled="{Binding Complete, Mode=OneWay}" />
              </StackLayout>
            </ViewCell>
          </DataTemplate>
        </ListView.ItemTemplate>
      </ListView>
      <StackLayout HorizontalOptions="Center"
                   Orientation="Horizontal">
        <Button BackgroundColor="Teal"
                Command="{Binding AddNewItemCommand}"
                Text="Add New Item"
                TextColor="White" />
      </StackLayout>
    </StackLayout>
  </ContentPage.Content>
</ContentPage>
```

Note that some bindings here are one-way.  This means that the value in the
view-model drives the value in the UI.  There is nothing within the UI that you
can do to alter the state of the underlying property.  Some bindings are two-way.
Doing something in the UI (for example, toggling the switch) alters the underlying
property.

This view is a little more complex.  It can be split into two parts - the list
at the top of the page and the button area at the bottom of the page.  The list
area uses a template to help with the display of each item.

Note that the `ListView` object has a "pull-to-refresh" option that I have wired
up so that when pulled, it calls the RefreshCommand.  It also has an indicator
that I have wired up to the IsBusy indicator.  Anyone who is familiar with the
iOS "pull-to-refresh" gesture can probably guess what this does.  

There is a view-model that goes along with the view (in `ViewModels\TaskListViewModel.cs`):

```csharp
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class TaskListViewModel : BaseViewModel
    {
        public TaskListViewModel()
        {
            Title = "Task List";
            RefreshList();
        }

        ObservableCollection<TodoItem> items = new ObservableCollection<TodoItem>();
        public ObservableCollection<TodoItem> Items
        {
            get { return items; }
            set { SetProperty(ref items, value, "Items"); }
        }

        TodoItem selectedItem;
        public TodoItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                SetProperty(ref selectedItem, value, "SelectedItem");
                if (selectedItem != null)
                {
                    Application.Current.MainPage.Navigation.PushAsync(new Pages.TaskDetail(selectedItem));
                    SelectedItem = null;
                }
            }
        }

        Command refreshCmd;
        public Command RefreshCommand => refreshCmd ?? (refreshCmd = new Command(async () => await ExecuteRefreshCommand()));

        async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var table = App.CloudService.GetTable<TodoItem>();
                var list = await table.ReadAllItemsAsync();
                Items.Clear();
                foreach (var item in list)
                    Items.Add(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskList] Error loading items: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        Command addNewCmd;
        public Command AddNewItemCommand => addNewCmd ?? (addNewCmd = new Command(async () => await ExecuteAddNewItemCommand()));

        async Task ExecuteAddNewItemCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Pages.TaskDetail());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskList] Error in AddNewItem: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task RefreshList()
        {
            await ExecuteRefreshCommand();
            MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
            {
                await ExecuteRefreshCommand();
            });
        }
    }
}
```

This is a combination of the patterns we have seen earlier.  The Add New Item
and Refresh commands should be fairly normal patterns now.  We navigate to the
detail page (more on that later) in the case of selecting an item (which occurs
when the UI sets the `SelectedItem` property through a two-way binding) and when
the user clicks on the Add New Item button.  When the Refresh button is clicked
(or when the user opens the view for the first time), the list is refreshed.  It
is fairly common to use an `ObservableCollection` or another class that uses the
`ICollectionChanged` event handler for the list storage.  Doing so allows the UI
to react to changes in the items.

Note the use of the `ICloudTable` interface here.  We are using the `ReadAllItemsAsync()`
method to get a list of items, then we copy the items we received into the `ObservableCollection`.

Finally, there is the TaskDetail page.  This is defined in the `Pages\TaskDetail.xaml`
file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="TaskList.Pages.TaskDetail"
             Title="{Binding Title}">
  <ContentPage.Content>
    <StackLayout Padding="10" Spacing="10">
      <Label Text="What should I be doing?"/>
      <Entry Text="{Binding Item.Text}"/>
      <Label Text="Completed?"/>
      <Switch IsToggled="{Binding Item.Complete}"/>
      <StackLayout VerticalOptions="CenterAndExpand"/>
      <StackLayout Orientation="Vertical" VerticalOptions="End">
        <StackLayout HorizontalOptions="Center" Orientation="Horizontal">
          <Button BackgroundColor="#A6E55E"
                  Command="{Binding SaveCommand}"
                  Text="Save" TextColor="White"/>
          <Button BackgroundColor="Red"
                  Command="{Binding DeleteCommand}"
                  Text="Delete" TextColor="White"/>          
        </StackLayout>
      </StackLayout>
    </StackLayout>
  </ContentPage.Content>
</ContentPage>
```

This page is a simple form with just two buttons that need to have commands
wired up.  However, this page is used for both the "Add New Item" gesture
and the "Edit Item" gesture.  As a result of this, we need to handle the
passing of the item to be edited.  This is done in the `Pages\TaskDetail.xaml.cs`
code-behind file:

```csharp
using TaskList.Models;
using TaskList.ViewModels;
using Xamarin.Forms;

namespace TaskList.Pages
{
    public partial class TaskDetail : ContentPage
    {
        public TaskDetail(TodoItem item = null)
        {
            InitializeComponent();
            BindingContext = new TaskDetailViewModel(item);
        }
    }
}
```

The item that is passed in from the `TaskList` page is used to create a
specific view-model for that item.  The view-model is similarly configured
to use that item:

```csharp
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class TaskDetailViewModel : BaseViewModel
    {
        ICloudTable<TodoItem> table = App.CloudService.GetTable<TodoItem>();

        public TaskDetailViewModel(TodoItem item = null)
        {
            if (item != null)
            {
                Item = item;
                Title = item.Text;
            }
            else
            {
                Item = new TodoItem { Text = "New Item", Complete = false };
                Title = "New Item";
            }
        }

        public TodoItem Item { get; set; }

        Command cmdSave;
        public Command SaveCommand => cmdSave ?? (cmdSave = new Command(async () => await ExecuteSaveCommand()));

        async Task ExecuteSaveCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                if (Item.Id == null)
                {
                    await table.CreateItemAsync(Item);
                }
                else
                {
                    await table.UpdateItemAsync(Item);
                }
                MessagingCenter.Send<TaskDetailViewModel>(this, "ItemsChanged");
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskDetail] Save error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        Command cmdDelete;
        public Command DeleteCommand => cmdDelete ?? (cmdDelete = new Command(async () => await ExecuteDeleteCommand()));

        async Task ExecuteDeleteCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                if (Item.Id != null)
                {
                    await table.DeleteItemAsync(Item);
                }
                MessagingCenter.Send<TaskDetailViewModel>(this, "ItemsChanged");
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskDetail] Save error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
```

The save command uses the `ICloudTable` interface again - this time doing
either `CreateItemAsync()` or `UpdateItemAsync()` to create or update the
item.  The delete command, as you would expect, deletes the item with the
`DeleteItemAsync()` method.

The final thing to note from our views is that I am using the `MessagingCenter`
to communicate between the TaskDetail and TaskList views.  If I change the item
in the `TaskDetail` list, then I want to update the list in the `TaskList` view.

Note that all the code we have added to the solution thus far is in the common
`TaskList` project.  Nothing is required for this simple example in a platform
specific project.  That isn't normal, as we shall see.

### Building the Client for Universal Windows

I tend to start by building the Universal Windows mobile client.  I'm using
Visual Studio, after all, and I don't need to use any emulator.  To build the
clients:

- Right-click on the **TaskList.UWP (Universal Windows)** project, then select **Set as StartUp Project**.
- Right-click on the **TaskList.UWP (Universal Windows)** project again, then select **Build**.
- Once the build is complete, Right-click on the **TaskList.UWP (Universal Windows)** project again, then select **Deploy**.
- Click on the **Local Machine** button in your command bar to run the application.

Here are the three screen screens we generated on Windows:

![Screen shots for Windows UWP][img12]

There are some problems with the UWP version.  Most notably, the "pull-to-refresh"
gesture does not exist, so we will need to set up an alternate gesture.  This
could be as easy as adding a refresh button right next to the Add New Item
button.  In addition, there is no indication of network activity - this manifests
as a significant delay between the TaskList page appearing and the data appearing
in the list.

Aside from this, I did do some styling work to ensure that the final version
looked like my mock-ups (with the exception of the UI form of the switch, which
is platform dependent).  If you want to see what I did to correct this, check out
the final version of [the Chapter 1 sample][11] on GitHub.  

If you need to build the project, ensure you redeploy the project after building.
It's a step that is easy to miss and can cause some consternation as you change
the code and it doesn't seem to have an effect on the application.

### Building the Client for Android

Building Android with Visual Studio is as easy as building the Universal Windows
version of the mobile client:

- Right-click on the **TaskList.Droid** project, then select **Set as StartUp Project**.
- Right-click on the **TaskList.Droid** project again, then select **Build**.

The button for running the application has a drop-down that is now significant.
It lets you choose the type of device that you will use.  Normally, this runs
either a 5" or 7" KitKat-based Android device.  There is some setup required for
the Visual Studio Emulator for Android.  The Visual Studio Emulator for Android
runs within Hyper-V, so you must install Hyper-V before use.  You get the following
error if you don't do this:

![Hyper-V is not installed][img13]

To install Hyper-V:

- Close all applications (your system will be rebooted during this process).
- Search for **Programs and Features**.
- Click on **Turn Windows features on or off** (in the left-hand menu).
- Expand the **Hyper-V** node.
- Check all the boxes below the **Hyper-V** node.  This will include Hyper-V Management Tools and Hyper-V Services.
- Click on **OK**.
- Your system will install the required pieces and then ask you to restart.  Click on **Restart now** when prompted.

Once you have done this, the next hurdle is to be added to the Hyper-V Administrators
security group.  It gets done for you (again - the first time you try to run the
application after installing Hyper-V).  Once it is done, close Visual Studio (again),
the log out and back in again.

As if that wasn't enough, the emulator also needs an Internet connection to
start.  

![The emulator requires and Internet connection to start][img14]

You should be able to just click on **Yes** or **OK** to enable the Internet
connection.  My laptop required a reboot before this would work, however.  In
addition, the process may request elevated privileges.

> If you want to run additional Android profiles before starting, run the **Visual Studio Emulator for Android** and download any additional profiles.  For example, if you wish to emulate something similar to a Samsung Galaxy S6, then download the profile for a 5.1" Marshmallow (6.0.0) XXHDPI Phone.

Finally the Visual Studio Emulator for Android starts when you click on the Run
button.  Fortunately, the setup of the emulator only has to be done once per
machine.  The Visual Studio Emulator for Android is also a superior emulator
to the standard Android Emulator, so this process is well worth going through.

> When testing the mobile client manually through the Visual Studio Emulator for Android, you are likely to need to rebuild the application.  You do not have to shut down the emulator between runs.  You can leave it running.  The application will be stopped and replaced before starting again.  This can significantly speed up the debug cycle since you are not waiting for the emulator to start each time.

Watch the Output window.  If the debugger won't connect or the application
won't start, you may need to restart your computer again to get the network
working.

> If your computer doesn't run Hyper-V well (or at all), then the emulator won't run well (or at all) either.  I find laptops to be particularly prone to this problem.  If this happens, you can always run the Google Emulator instead.  Build the application as normal.  You will find the APK file to install in `...\TaskList.Droid\bin\Debug`. Fortunately, there are lots of resources that show how to do this.  You can find the answer on [Stack Overflow][13]

If everything is working, you should see the Visual Studio Emulator for Android
display your mobile client:

![Visual Studio Emulator for Android Final][img16]

> You can also build the Android version on a mac with Xamarin Studio.  However, I find that version mismatches between Mono (which is used on the mac) and Visual Studio - particularly in reference to the version of the .NET framework - cause issues when swapping between the two environments.  For best results, stay in one environment.

Note that the task list view is a "light" style and the rest of the app is a
"dark" style.  This is because the default styling on an Android device is
dark.  We are using the default styling on two of the pages and specifying
colors on the list page.  Fortunately, Xamarin Forms allows for [platform-specific
styling][14].  The [final sample][11] has platform-specific styling for the
list page.

### Building the Client for iOS

Finally, we get to the iOS platform.  You will need to ensure your Mac is turned
on and accessible, that it has XCode installed (and you have run XCode once so
that you can accept the license agreement), and it has Xamarin Studio installed.

When you created the projects, you were asked to link Visual Studio to your mac.  
This linkage is used for building the project.  In essence, the entire project
is sent to the Mac and the build tools that are supplied with Xamarin Studio
will be used to build the project.

- Right-click on the **TaskList.iOS** project and select **Set as StartUp Project**.
- Right-click on the **TaskList.iOS** project and select **Build**.

You knew it was not going to be that easy, right?  Here are the errors that I
received when building for the first time:

![iOS First Build Errors][img17]

There are two errors right at the top.  Let's cover the first one.  The error
about _Build Action 'EmbeddedResource' is not supported_ is an annoying one.
The fix is to do the following:

1. Set the iOS project as the StartUp project.
2. Go through each project, expand the **References** node and ensure that there are no references with a warning (a little yellow triangle).  If there are - fix those first.  Generally, this is fixed by either using the **Restore NuGet Packages** option or removing the reference and then adding it again from NuGet.
3. Close the solution.
4. Re-open the solution.  You don't need to close Visual Studio to do this.
5. Right-click on the iOS project and select **Clean**.
6. Right-click on the iOS project and select **Rebuild**.

Once you have done this sequence, the error should go away.

The error about _No valid iOS code signing keys found in keychain_ is because
we have not yet signed up for an Apple Developer Account and linked it to our
Mac development environment.

- Go to the [Apple Developer Center][15].
- Click on **Account** in the top navigation bar.
- If you haven't got an Apple ID yet, create one first.
- If you have go an Apple ID, then log in.

There are a sequence of sign-up prompts in both cases (first for creating your
Apple ID and secondly for signing up for the Apple Developer program).  Once
you have gone through this process, you are registered as an Apple Developer.

> If you want to distribute your apps on the Apple App Store or get access to the beta bits, then you might consider signing up for the Apple Developer Program.  The Apple Developer Program is an additional cost and is not required for developing iOS apps that are not for distribution.

Once you have created your account and enabled it as a developer account, open
up XCode.  You will need to get beyond the first screen; I just create a
Playground for this purpose.  Go to **Preferences...**, then **Account** and
click on the **+** in the bottom-left corner of the window:

![Adding an Apple ID to XCode][img18]

Sign in with the same account you used to sign up for the developer account.

![The Apple ID in XCode][img19]

## Some Final Thoughts



[img1]: img/ch1/pic1.PNG
[img2]: img/ch1/pic2.PNG
[img3]: img/ch1/pic3.PNG
[img4]: img/ch1/xamarinforms-templates.PNG
[img5]: img/ch1/new-xf-project.PNG
[img6]: img/ch1/win10-developermode.PNG
[img7]: img/ch1/pick-uwp-platform.PNG
[img8]: img/ch1/xamarin-mac-agent.PNG
[img9]: img/ch1/xamarin-mac-login.PNG
[img10]: img/ch1/xf-newsolution.PNG
[img11]: img/ch1/nuget-mobileinstall.PNG
[img12]: img/ch1/uwp-final.PNG
[img13]: img/ch1/need-hyperv.PNG
[img14]: img/ch1/emulator-setup-internet.PNG
[img15]: img/ch1/avd-mac.PNG
[img16]: img/ch1/droid-final.PNG
[img17]: img/ch1/ios-build-errors.PNG
[img18]: img/ch1/xcode-add-appleid.PNG
[img19]: img/ch1/xcode-appleid.PNG

[int-data]: ./3_data.md

[1]: https://azure.microsoft.com/en-us/documentation/learning-paths/appservice-mobileapps/
[2]: https://mockingbot.com/app/RQe0vlW0Hs8SchvHQ6d2W8995XNe8jK
[3]: https://mockingbot.com/
[4]: https://msdn.microsoft.com/en-us/data/ef
[5]: http://www.odata.org/documentation/odata-version-3-0/
[6]: https://portal.azure.com/
[7]: https://guidgenerator.com/
[8]: https://visualstudiogallery.msdn.microsoft.com/e1d736b0-5531-4eee-a27a-30a0318cac45
[9]: https://developer.xamarin.com/guides/ios/getting_started/installation/windows/connecting-to-mac/
[10]: https://developer.xamarin.com/guides/xamarin-forms/creating-mobile-apps-xamarin-forms/
[11]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/blob/master/Chapter1
[12]: https://github.com/
[13]: https://stackoverflow.com/questions/3480201/how-do-you-install-an-apk-file-in-the-android-emulator/3480235#3480235
[14]: https://jfarrell.net/2015/02/07/platform-specific-styling-with-xamarin-forms/
[15]: https://developer.apple.com/
