# Your First Mobile App on a Mac

There is a lot of detail to absorb about the possible services that the mobile
client can consume and I will go into significant depth on those subjects.
First, wouldn't it be nice to write some code and get something working?
Microsoft Azure has a great [first-steps tutorial][1] that takes you via the
quickest possible route from creating a mobile backend to having a functional
backend.  I would like to take things a little slower so that we can understand
what is going on while we are doing the process.  We will have practically the
same application at the end.  The primary reason for going through this slowly
is to ensure that all our build and run processes are set up properly.  If this
is the first mobile app you have ever written, you will see that there are quite
a few things that need to be set up.  This chapter covers the set up required
for a MacOS computer.  If you wish to develop your applications on a Windows PC, then skip
to the [prior section][int-firstapp-pc].

The application we are going to build together is a simple task list.  The
mobile client will have three screens - an entry screen, a task list and a task
details page.  I have mocked these pages up using [MockingBot][2].

!!! tip
    Mocking your screens before you start coding is a great habit to get into.
    There are some great tools available including free tools like [MockingBot][3].
    Doing mockups before you start coding is a good way to prevent wasted time later
    on.

![Application Mockups for the Task List][img1]

!!! tip
    If you are using iOS, then you may want to remove the back button as the style
    guides suggest you don't need one.  Other platforms will need it though, so it's
    best to start with the least common denominator.  It's the same reason I add a
    refresh button even though it's only valid on Windows Phone!

My ideas for this app include:

* Tapping on a task title in the task list will bring up the details page.
* Toggling the completed link in the task list will set the completed flag.
* Tapping the spinner will initiate a network refresh.

Now that we have our client screens planned out, we can move onto the thinking
about the mobile backend.

## The Mobile Backend

The mobile backend is an ASP.NET WebApi that is served from within Azure App
Service: a highly scalable and redundant web hosting facility that supports all
the major web languages (like ASP.NET, Node, PHP and Python).  Azure Mobile
Apps is an SDK (which is available in ASP.NET and Node) that runs on top of
Azure App Service.

### Creating a Simple Azure Mobile Apps Backend

To get started:

1. Fire up Xamarin Studio
2. Add a new project with File -> New Solution...
3. In the **New Project** window:

    - Choose Other -> ASP.NET and select **Empty ASP.NET Project**.
    - Click Next.

4. In the **Configure your Web project** window:
    - Check the **Web API** checkbox.
    - Click Next.

5. In the **Configure your new project** window:
    - Enter **Backend** for the for the Project Name and **Chapter1** for the Solution name.
    - Pick a suitable directory for the Location field.
    - Click on Create.

At this point, Xamarin Studio will create your backend project.

You may need to accept licenses for several packages to continue with
the project creation.  As Xamarin Studio doesn't have templates for all
of the same ASP.NET projects that Visual Studio does, we'll need to do
some additional work to set our project up.

First, we need to change our project to target .NET 4.6.  Right click on
your **Backend** project in the Solution Explorer and choose Options.  Under
the **Build** section, select **General**.  Under the Target Framework dropdown
select .NET Framework 4.6.1:

![Changing the Target Framework][img11]

Click **OK** to accept the change and close the Project Options.

Next we'll install multiple NuGet packages.  Expand the **Backend** project
in the Solution Explorer and right click on **Packages** and then select
**Add Packages**.  Find and add the following packages:

*  Azure Mobile .NET Server SDK
*  Azure Mobile .NET Server Tables
*  Azure Mobile .NET Server CORS
*  Azure Mobile .NET Server Notifications
*  Azure Mobile .NET Server Authentication
*  Azure Mobile .NET Server Home
*  Azure Mobile .NET Server Quickstart
*  Azure Mobile .NET Server Entity
*  Microsoft.OWIN.Host.SystemWeb

!!! tip
    The Azure Mobile .NET Server Quickstart NuGet package has all the other packages
    as dependencies.  Add the Quickstart package first to save yourself some time.

You should also take the opportunity to update any NuGet packages that were
automatically added to the project.  To do so, right click on **Packages** and
choose **Update**.

Next you'll need to add the following folders to the **Backend** project.  Right
click on **Backend** and select Add -> New Folder and create the following:

* Controllers
* DataObjects
* Models

Next you'll need to remove the following files that were created as part of the template.
Right click on each of the following files and choose Remove and click the Delete button:

* App_Start/WebApiConfig.cs
* Global.asax

Now we can add the files that will consist of our backend.  We'll start by
creating the three files that will handle projecting a single table in our
database - the TodoItem table - into the mobile realm with the aid of an
opinionated [OData v3][5] feed.  To that end, we need three items:

* A `DbSet<>` within the `DbContext`
* A Data Transfer Object (or DTO)
* A Table Controller


Start by right clicking on `DataObjects` and choose Add -> New File.  Select General -> Empty Class and name it
`TodoItem.cs`.  As we're building a task list application, this is the DTO
for our TodoItems:

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

Next, we'll create our `DbContext` which uses [Entity Framework][4] to deal with creating
our Database Model when we upload our backend to Azure.  Right click on **Models** and choose
Add -> New File.  Select General -> Empty Class and name it `MobileServiceContext.cs`.
Add the following code:

```csharp
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;
using Backend.DataObjects;

namespace Backend.Models
{
    public class MobileServiceContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        //
        // If you want Entity Framework to alter your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
        //
        // To enable Entity Framework migrations in the cloud, please ensure that the
        // service name, set by the 'MS_MobileServiceName' AppSettings in the local
        // Web.config, is the same as the service name when hosted in Azure.

        private const string connectionStringName = "Name=MS_TableConnectionString";

        public MobileServiceContext() : base(connectionStringName)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
        }
    }
}
```

Next we'll create the controller for our TodoItem table.  Right click on **Controllers**
and choose Add -> New File.  Select General -> Empty Class and name it
`TodoItemController.cs`.  Add the following code:

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
It handles all the OData capabilities for us and exposes these as REST endpoints
within our WebAPI.  This means that the actual code for this controller is tiny - just 12 lines of code.

!!! info
    [OData][5] is a specification for accessing table data on the Internet.  It provides
    a mechanism for querying and manipulating data within a table.  Entity Framework is a
    common data access layer for ASP.NET applications.

Moving on, we'll create the startup file for our backend.  Right click on **App_Start**
-> Add -> New File.  Select General -> Empty Class and name it
`Startup.MobileApp.cs`.

!!! info
    If you name a file in Xamarin Studio with a period in it without including the specific
    extension (.cs) at the end, Xamarin Studio will not automatically set the file up
    to be compiled correctly.

This class handles initializing our mobile application backend:

```csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Backend.DataObjects;
using Backend.Models;
using Owin;

namespace Backend
{
    public partial class Startup
    {
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
    }

    public class MobileServiceInitializer : CreateDatabaseIfNotExists<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext context)
        {
            List<TodoItem> todoItems = new List<TodoItem>
            {
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "First item", Complete = false },
                new TodoItem { Id = Guid.NewGuid().ToString(), Text = "Second item", Complete = false }
            };

            foreach (TodoItem todoItem in todoItems)
            {
                context.Set<TodoItem>().Add(todoItem);
            }

            base.Seed(context);
        }
    }
}
```

If you were to create a new Mobile App based application in Visual Studio or download the
quickstart application from the Azure portal, the startup
would be considerably inflated with additional functionality for things like
authentication and push notifications.  Currently we have it set up to only implement
our data layer.

There is another method in the `App_Start\Startup.MobileApp.cs` file for
seeding data into the database for us.  We can leave that alone for now, but
remember it is there in case you need to seed data into a new database for
your own backend.

!!! info
    When we refer to "seeding data" into a database, this means that we are going to introduce
    some data into the database so that we aren't operating on an empty database. The data
    will be there when we query the database later on.


Finally we need to add a startup class for our ASP.NET
application.  Right click on **Backend** -> Add -> New File.  Select General ->
Empty Class and name it `Startup.cs`.  The purpose of this class is just to kick off
the configuration of our mobile app backend:

```csharp
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Backend.Startup))]

namespace Backend
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}
```

Our last step in our backend before publishing it is to edit the `web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<!--
Web.config file for TestProject.

The settings that can be used in this file are documented at
http://www.mono-project.com/Config_system.web and
http://msdn2.microsoft.com/en-us/library/b5ysx397.aspx
-->
<configuration>
  <configSections>
    <section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
    <!-- For more information on Entity Framework configuration, visit http://go.microsoft.com/fwlink/?LinkID=237468 -->
    <!-- For more information on Entity Framework configuration, visit http://go.microsoft.com/fwlink/?LinkID=237468 -->
  </configSections>
  <connectionStrings>
    <add name="MS_TableConnectionString" connectionString="Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\aspnet-MobileAppsBookTest-20160923095604.mdf;Initial Catalog=aspnet-MobileAppsBookTest-20160923095604;Integrated Security=True;MultipleActiveResultSets=True" providerName="System.Data.SqlClient" />
  </connectionStrings>
  <appSettings>
    <add key="PreserveLoginUrl" value="true" />
    <!-- Use these settings for local development. After publishing to your
    Mobile App, these settings will be overridden by the values specified
    in the portal. -->
    <add key="MS_SigningKey" value="Overridden by portal settings" />
    <add key="EMA_RuntimeUrl" value="Overridden by portal settings" />
    <!-- When using this setting, be sure to add matching Notification Hubs connection
    string in the connectionStrings section with the name "MS_NotificationHubConnectionString". -->
    <add key="MS_NotificationHubName" value="Overridden by portal settings" />
  </appSettings>
  <system.web>
    <customErrors mode="Off" />
    <httpRuntime targetFramework="4.6.1" />
    <compilation debug="true" targetFramework="4.6.1">
      <assemblies />
    </compilation>
  </system.web>
  <system.webServer>
    <validation validateIntegratedModeConfiguration="false" />
    <modules runAllManagedModulesForAllRequests="true" />
    <handlers>
      <remove name="ExtensionlessUrlHandler-Integrated-4.0" />
      <remove name="OPTIONSVerbHandler" />
      <remove name="TRACEVerbHandler" />
      <add name="ExtensionlessUrlHandler-Integrated-4.0" path="*." verb="*" type="System.Web.Handlers.TransferRequestHandler" preCondition="integratedMode,runtimeVersionv4.0" />
    </handlers>
  </system.webServer>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1" xmlns:bcl="urn:schemas-microsoft-com:bcl">
      <dependentAssembly>
        <assemblyIdentity name="System.Spatial" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.7.0.0" newVersion="5.7.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web.Http" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.2.3.0" newVersion="5.2.3.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Net.Http.Formatting" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.2.3.0" newVersion="5.2.3.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Data.Edm" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.7.0.0" newVersion="5.7.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Data.OData" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.7.0.0" newVersion="5.7.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-9.0.0.0" newVersion="9.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Owin" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-3.0.1.0" newVersion="3.0.1.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.IdentityModel.Tokens.Jwt" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.0.0.127" newVersion="5.0.0.127" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web.Http.OData" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.7.0.0" newVersion="5.7.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="AutoMapper" publicKeyToken="be96cd2c38ef1005" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.1.1.0" newVersion="5.1.1.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
  <entityFramework>
    <defaultConnectionFactory type="System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework" />
    <providers>
      <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
    </providers>
  </entityFramework>
</configuration>
```

Choose **Build All** from the **Build** menu and ensure your project compiles
without errors.

### Building an Azure App Service for Mobile Apps

The next step in the process is to build the resources on Azure that will
run your mobile backend.  Start by logging into the [Azure Portal][6], then
follow these instructions:

1. Click on the big **+ New** button in the top-left corner.
2. Click on **Web + Mobile**, then **Mobile App**.
3. Enter a unique name in the **App name** box.

    !!! tip
        Since the name doesn't matter and it has to be unique, you can use [a
        GUID generator][7] to generate a unique name. GUIDs are not the best
        names to use when you need to actually find resources, but using GUIDS
        prevents conflicts when deploying, so I prefer them as a naming scheme.
        You can prefix the GUID  (example: chapter1-GUID) to aid in discovery
        later on.  Generally, the first four digits of a GUID are enough to
        identify individual resources.

4. If you have more than one subscription (for example, you have a trial
   and an MSDN subscription), then ensure you select the right subscription
   in the **Subscription** drop-down.
5. Select **Create new** under resource group and enter a name for this
   mobile application.

    !!! info "Resource Groups"
        Resource groups are great for grouping all the resources associated with a
        mobile application together.  During development, it means you can delete
        all the resources in one operation.  For production, it means you can see how
        much the service is costing you and how the resources are being used.

6. Finally, select or create a new **App Service Plan**.

    !!! info "App Service Plan"
        The App Service Plan is the thing that actually bills you - not the web or
        mobile backend.  You can run a number of web or mobile backends on the same
        App Service Plan.

    I tend to create a new App Service Plan for each mobile application.  This
    is because the App Service Plan lives inside the Resource Group that you
    create.  The process for creating an App Service Plan is straight forward.
    You have two decisions to make.  The first decision is where is the service
    going to run.  In a production environment, the correct choice is "near your
    customers".  "Close to the developers" is a good choice during development.
    Unfortunately, neither of those is an option you can actually choose in the
    portal, so you will have to translate into some sort of geographic location.
    With 22 regions to choose from, you have a lot of choice.

    The second decision you have to make is what to run the service on; also
    known as the Pricing tier.   If you click on **View all**, you will see you
    have lots of choices.  F1 Free and D1 Shared, for example, run on shared
    resources and are CPU limited. You should avoid these as the service will
    stop responding when you are over the CPU quota.  That leaves Basic,
    Standard, and Premium.  Basic has no automatic scaling and can run up to 3
    instances - perfect for development tasks.  Standard and Premium both have
    automatic scaling, automatic backups, and large amounts of storage; they
    differ in features: the number of sites or instances you can run on them,
    for example.  Finally, there is a number after the plan.  This tells you how
    big the virtual machine is that the plan is running on.  The numbers differ
    by number of cores and memory.

    For our purposes, a F1 Free site is enough to run this small demonstration
    project.  More complex development projects should use something in the
    Basic range of pricing plans.  Production apps should be set up in Standard
    or Premium pricing plans.

7. Once you have created your app service plan and saved it, click on **Create**.

The creation of the service can take a couple of minutes.  You can monitor the
process of deployment by clicking on the Notifications icon.  This is in the top
bar on the right-hand side and looks like a Bell.  Clicking on a specific
notification will provide more information about the activity.  Once you have
created your app service, the App Service blade will open.

!!! info
    What's the difference between a Web App, a Mobile App and an API App?  Not a
    lot.  The type determines which Quick start projects are available in the Quick
    start menu under **All settings**.  Since we selected a Mobile app, a set of
    starter client projects for mobile devices will be presented.

We will also want a place to store our data.  This role is taken on by a
SQL Azure instance.  We could link an existing database if we had one
defined.  However, we can also create a test database.

!!! tip
    Creating a Test Database through the App Service Data Connections (as
    I describe here) allows you to create a free database.  This option is
    not normally available through other SQL database creation flows.

1.  Click on **Resource groups** in the left hand side menu.
2.  Click on the resource group you created.
3.  Click on the App Service your created.

    !!! tip
        If you pinned your App Service to the dashboard, you can click on the
        pinned App Service instead.  It will bring you to the same place.

4.  Click on **Data connections** in the **MOBILE** menu.
5.  Click on **Add**.

    - In the **Type** box, select **SQL Database**.
    - Click on the unconfigured **SQL Database** link:

    ![Data Connection][img6]

    - Enter a name for the database (like **chapter1-db**).
    - Select a Pricing Tier (look for **F Free** at the bottom).
    - Click on the unconfigured **Server**.

    ![SQL Server Configuration][img7]

    - Enter a unique name for the server (a GUID is a good idea here).
    - Enter a username and password for the server.
    - Make sure the Location for your database server is the same as your
    Mobile App.
    - Click on **Select** to close the **New Server** blade.
    - Click on **Select** to close the **New Database** blade.
    - An error may appear asking you to set the Database Connection string,
    if so, click on the Database Connection and then click **OK** on it's blade.
    - Click on **OK** to close the **Add Data Connection** blade.


This produces another deployment step that creates a SQL Server and a SQL
database with your settings.  Once complete, the connection
**MS_TableConnectionString** will be listed in Data Connections blade.

![Successful Data Connection][img8]

!!! tip
    If you want a completely free mobile backend, search for the **Mobile
    Apps Quickstart** in the Azure Marketplace.  This template does not
    require a database.  It relies on a Node backend, however, so you won't
    be developing a C# backend.

### Deploying the Azure Mobile Apps Backend

Publishing a Mobile App backend is very integrated into Visual Studio.  Since
we're developing with Xamarin Studio, we'll need to do things a bit differently.
There are a number of potential deployment options including FTP, connecting
to a Continuous Integration server, connecting a Dropbox folder, or creating a local git repository.  To
keep things simple, we'll use an adminstration site named Kudu (also called
SCM) to copy the files over.  Once we're ready, we'll need to collect the compiled
application files since Xamarin doesn't have any publish functionality.

1. Return to the browser and the Azure Portal.
2. Go to the Settings blade for your Mobile App.
3. Click on **Advanced Tools** in the **DEVELOPMENT TOOLS** menu.
4. Click on **Go** in the **Advanced Tools** blade.

    ![Advanced Tools][img9]

5. The page that loads should match https://{YourMobileApp}.scm.azurewebsites.net/.
6. Select the **Debug Console** menu from the top and choose **CMD**.
7. Within the file structure listing, click on **site**.
8. Click on **wwwroot**.
9. You should see a **hostingstart.html** file here.  Click the circle with a minus
symbol in it to the left of that file and confirm the dialog to delete this file.
10. On your computer, navigate to the folder that contains your Mobile App Backend.
11. Select the following folder and files:

    - bin
    - packages.config
    - Web.config

12. Drag and drop those files into the browser window above the Console.
13. A progress indicator should appear near the top right.  Upon completion you
should see the files appear in the file list:

    ![Files deployed][img10]

In the browser navigate to the URL for your Mobile App.  This should match the
format https://{YourMobileApp}.azurewebsites.net/.



Add `/tables/todoitem?ZUMO-API-VERSION=2.0.0`
to the end of the URL.  This will show the JSON contents of the table that we
defined in the backend.

!!! info
    You will see the word ZUMO all over the SDK, including in optional HTTP headers
    and throughout the SDK source code.  ZUMO was the original code name within Microsoft
    for A<b>ZU</b>re <b>MO</b>bile.

## The Mobile Client

Now that the mobile backend is created and deployed, we can move onto the client
side of things.  As we're using Xamarin Studio, we should already have everything
we need installed to proceed with creating a Xamarin.Forms application for
both Android and iOS.

!!! info
    When you compile a Xamarin.Forms application for a specific platform, you are
    producing a true native application for that platform - whether it be iOS,
    Android or Windows.

### Creating a Simple Mobile Client with Xamarin

Right-click on the **Chapter1** solution and select **Add** -> **Add New Project**.
This will bring up the familiar New Project dialog.  The project you want is
under **Multiplatform** -> **App**, and is called **Forms App**.  Select that
and click **Next**.  Give the project a name such as **TaskList**, ensure Android and iOS are both
selected, select **Use Portable Class Library** and click Next.

  ![Creating our Client Projects][img12]

On the next screen, leave all of the default values and click Create.

Once the setup is complete, you will see that three new projects have been
created: a common library which you named plus one project for each platform
that has been chosen.  Since we chose both Android and iOS, we get
three projects:

  ![The TaskList Project Layout][img13]

Most of our work will happen in the common library.  However, we can introduce
platform-specific code at any point.  The platform-specific code is stored in
the platform-specific project.

There is one final item we must do before we leave the set up of the project.
There are a number of platform upgrades that inevitably have to happen.  The
Xamarin Platform is updated much more often than the project templates in Xamarin
Studio - the updates are released via NuGet: the standard method of distributing
libraries for .NET applications.  In addition to the inevitable Xamarin Platform
updates, we also will want to add the following NuGet packages:

*  Microsoft.Azure.Mobile.Client v2.0.0 or later
*  Newtonsoft.Json v6.0.3 or later

!!! warn
    Although it is tempting, do not include a v1.x version of the Mobile Client.
    This is for the earlier Azure Mobile Services.  There are many differences between
    the wire protocols of the two products.

You can start by updating the existing NuGet packages by right-clicking on the
Packages folder in each project and selecting **Update**.

Next we can install the NuGet packages by right-clicking on the Packages folder in each
project and selecting **Add Packages...**.

  ![Manage NuGet Packages][img14]

You must install the updates and the new NuGet packages on all three projects.
This involves repeating the same process for each client project in your
solution.

!!! info
    Android generally has more updates than the other platforms.  Ensure that you
    update the main Xamarin.Forms package and then refresh the update list.  This will
    ensure the right list of packages is updated.

### Building the Common Library

There are two parts that we must concentrate on within the common library.  The
first is the connection to Azure Mobile Apps and the second is in the pages
that the user interacts with.  In both cases, there are best practices to observe.
Start by adding the following folders to your Portable Class Library project:

* Abstractions
* Models
* Pages
* Services
* ViewModels

#### Building an Azure Mobile Apps Connection

We will rely on interfaces for defining the shape for the class for any service
that we interact with.  This is really not important in small projects like this
one.  This technique allows us to mock the backend service, as we shall see
later on.  Mocking the backend service is a great technique to rapidly iterate
on the front end mobile client without getting tied into what the backend is doing.

Let's start with the cloud service - this will be defined in a new interface
`Abstractions\ICloudService.cs`.  It is basically used for initializing
the connection and getting a table definition:

```csharp
namespace TaskList.Abstractions
{
    public interface ICloudService
    {
        ICloudTable<T> GetTable<T>() where T : TableData;
    }
}
```

There is a dependent implementation here: the `ICloudTable` generic interface.  This
represents a CRUD interface into our tables and will be defined in `Abstractions\ICloudTable.cs`:

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

        Task<ICollection<T>> ReadAllItemsAsync();
    }
}
```

The `ICloudTable<T>` interface defines the normal CRUD operations: Create, Read,
Update, and Delete.  However, it does so asynchronously.  We are dealing with network
operations in general so it is easy for those operations to tie up the UI thread
for an appreciable amount of time.  Making them async provides the ability to
respond to other events.  I also provide a `ReadAllItemsAsync()` method that
returns a collection of all the items.

There are some fields that every single record within an Azure Mobile Apps table
provides.  These fields are required for offline sync capabilities like incremental
sync and conflict resolution.  The fields are provided by a new abstract base class
on the client called `Abstractions\TableData`:

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

As we will learn when we deal with [table data][int-data], these fields need to
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

!!! warn "Ensure you use HTTPS"
    If you copy the URL on the Overview page of your App Service, you will get the http
    version of the endpoint.  You must provide the https version of the endpoint when
    using App Service.  The http endpoint redirects to https and the standard HttpClient
    does not handle redirects.

The Azure Mobile Apps Client SDK takes a lot of the pain out of communicating
with the mobile backend that we have already published.  Just swap **my-backend** out for the
name of your mobile backend and the rest is silently dealt with.

!!! warn
    The name `Microsoft.WindowsAzure.MobileServices` is a hold-over from the old Azure
    Mobile Services code-base.  Don't be fooled - clients for Azure Mobile Services are
    not interchangeable with clients for Azure Mobile Apps.

We also need a concrete implementation of the `ICloudTable<T>` interface
(in `Services\AzureCloudTable.cs`):

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

        public async Task DeleteItemAsync(T item)
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

!!! tip
    You can use a shorthand (called a lambda expression) for methods with only one line.
    For instance, the delete method could just as easily have been written as:

    ```
    public async Task DeleteItemAsync(T item) => await table.DeleteAsync(item);
    ```

    You may see this sort of short hand in samples.

We also need to create the model that we will use for the data.  This should
look very similar to the model on the server - including having the same name
and fields.  In this case, it's `Models\TodoItem.cs`:

```csharp
using TaskList.Abstractions;

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

        // There are life cycle methods here...
    }
}
```

We haven't written `Pages.EntryPage` yet, but that's coming.  The original `App.cs`
class file had several methods for handling life cycle events like starting, suspending,
or resuming the app.  I did not touch those methods for this example.

#### Building the UI for the App

Earlier, I showed the mockup for my UI.  It included three pages - an entry
page, a list page, and a detail page.  These pages have three elements - a
XAML definition file, a (simple) code-behind file, and a view model.

!!! info
    This book is not intending to introduce you to everything that there is to know
    about Xamarin and UI programming with XAML.  If you wish to have that sort of introduction,
    then I recommend reading the excellent book by Charles Petzold:
    [Creating Mobile Apps with Xamarin.Forms][8].

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

Creating a XAML file is relatively simple.  We already created a `Pages` directory to
hold the pages of our application.  Right-click on the `Pages` directory in
the solution explorer and choose **Add** -> **New File...**.  In the **Add New File**
dialog, pick **Forms** -> **Forms ContentPage Xaml**.  Name the
new page `EntryPage`.  This will create two files - `EntryPage.xaml` and
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

There are two bindings.  These are bound from the view-model.  We've already seen
the Title property - this is a text field that specifies the title of the page.
The other binding is a login command.  When the button is tapped, the login command
will be run.  We'll get onto that in the view-model later.

The other file created is the code-behind file.  Because we are moving all
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
bind all the bindings to a new instance of the view model.

Speaking of which, the view-model just needs to handle the login click.  Note that
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

The code behind for the TaskList can be found in `Pages\TaskList.xaml.cs`:

```csharp
using TaskList.ViewModels;using Xamarin.Forms;
namespace TaskList.Pages
{
	public partial class TaskList : ContentPage
	{
		public TaskList()
		{
			InitializeComponent();
			BindingContext = new TaskListViewModel();
		}
	}
}
```

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

### Building the Client for Android

Now we're ready to build our client applications.  We'll start with the Android version.  Prior
to running the application, we need to make two additional changes.  Go to your Android
project and open the **MainActivity.cs** file.  In the **OnCreate** method we need to add
an initalizer for our Mobile Apps SDK:

```csharp
protected override void OnCreate(Bundle bundle)
{
    TabLayoutResource = Resource.Layout.Tabbar;
    ToolbarResource = Resource.Layout.Toolbar;
    base.OnCreate(bundle);

    Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

    global::Xamarin.Forms.Forms.Init(this, bundle);
    LoadApplication(new App());
}
```

Finally, as Android has an explicit permission model (this has somewhat changed in the latest
version of Android), we need to say the application requires the Internet permission.  Right-click
on the Android project and go to **Options**.  Select **Android Application** from under the
**Build** section.  At the bottom of the options panel, you'll see a list of permissions.
Find Internet and check it and then click the OK button.

![Adding the Intenet permission][img15]

Next we need to configure the solution to run the Android project.

- Right-click on the **TaskList.Droid** project, then select **Set as StartUp Project**.
- Right-click on the **TaskList.Droid** project again, then select **Build TaskList.Droid**.

The drop-down between the run button in the top left of Xamarin Studio and the Build status
at the top of Xamarin Studio, now allows you to choose an emualtor or device to run your app
against.  By default, Xamarin Studio will create several emulators for you.  You can also use
the **Manage Google Emulators...** option to create additional Android Virtual Devices (AVDs)
and download other images online.

!!! tip
    When testing the mobile client manually through the Android Emulator,
    you are likely to need to rebuild the application.  You do not have to
    shut down the emulator between runs.  You can leave it running.  The application
    will be stopped and replaced before starting again.  This can significantly speed
    up the debug cycle since you are not waiting for the emulator to start each time.

Watch the Output window.  If the debugger won't connect or the application
won't start, you may need to restart your computer or the emulator to get the network
working.

If everything is working, you should see the Android Emulator
display your mobile client:

![Android app running on emulator][img16]

!!! warn
    You can also build the Android version on Windows with Visual Studio.  However, I find
    that version mismatches between Mono (which is used on the mac) and Visual Studio - particularly
    in reference to the version of the .NET framework - cause issues when swapping between the
    two environments.  For best results, stay in one environment.

Note that the task list view is a "dark" style and the rest of the app is a
"light" style.  This is because the default styling on an Android device is
light.  We are using the default styling on two of the pages and specifying
colors on the list page.  Fortunately, Xamarin Forms allows for [platform-specific
styling][10].  The [final sample][9] has platform-specific styling for the
list page.

### Building the Client for iOS

With Android done, we can now turn to the iOS platform.  Like we did for Android, we must
first initalize our Mobile Apps SDK for our platform.  Open the **AppDelegate.cs** file in
your iOS project.  In the FinishedLaunching method, we will initalize our SDK:

```csharp
public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
    global::Xamarin.Forms.Forms.Init();

    Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

    LoadApplication(new App());
    return base.FinishedLaunching(app, options);
}
```

Now we can build and run our app:

- Right-click on the **TaskList.iOS** project and select **Set as StartUp Project**.
- Right-click on the **TaskList.iOS** project and select **Build TaskList.iOS**.

If you have never used Xamarin Studio to build and run an iOS app before, it is possible
that you will receive an error having to do with code signing keys, provisioning profiles,
or signing identities.  If so it may be  because you have not yet signed up for an Apple
Developer Account and linked it to your Mac development environment.

- Go to the [Apple Developer Center][11].
- Click on **Account** in the top navigation bar.
- If you haven't got an Apple ID yet, create one first.
- If you have go an Apple ID, then log in.

There are a sequence of sign-up prompts in both cases (first for creating your
Apple ID and secondly for signing up for the Apple Developer program).  Once
you have gone through this process, you are registered as an Apple Developer.

!!! info
    If you want to distribute your apps on the Apple App Store, run on real devices or
    get access to the beta bits, then you might consider signing up for the Apple Developer
    Program.  The Apple Developer Program is an additional cost and is not required for
    developing iOS apps that are only run on the iOS simulator.

Once you have created your account and enabled it as a developer account, open
up Xcode.  Go to **Preferences...**, then **Account** and
click on the **+** in the bottom-left corner of the window and pick **Add Apple ID...**:

![Adding an Apple ID to Xcode][img2]

Sign in with the same account you used to sign up for the developer account.

![The Apple ID in Xcode][img3]

Click on the **View Details** button.  This will bring up the Signing Identities
list.  For a free account, it looks like this:

![Xcode Signing Identities][img4]

Click on the Create button next to **iOS Development**.  Once the UI comes back,
click on **Done**.  For more information on this process, refer to the [Apple Documentation][12].

You can close Xcode at this point.  Return to Xamarin Studio, right-click on the
**TaskList.iOS** project and build again.  This will (finally!) build the application for you.

!!! tip
    Getting an error about _Provisioning Profiles_ not being available?  This is because
    you are building for a real device instead of the simulator.  In order to build for a
    real device, you must have a linked Apple Developer Program.  To get around this, select
    a Simulator before building.

You can now select from several simulator options from the drop-down to the left of the
build status.
You should only use **Device** if you have signed up for the Apple Developer Program.  Pick
one of the simulator options like the **iPhone 6 iOS 10.0** simulator, then click on it
to run the simulator.

The final product screens look like this:

![iOS Final Screens][img5]

## Some Final Thoughts

If you have got through the entire process outlined in this Chapter and built the application
for each platform, then congratulations.  There are a lot of places where things can go
wrong.  You are really integrating the build systems across Android, iOS, Xamarin, Xcode,
and Azure.

Fortunately, once these are set up, it's likely that they will continue working and you
won't have to think too much about them again. The Android and iOS build tools and simulators
will just work.

The following 7 chapters each take one aspect of the cloud services that can be provided to
mobile apps and explores it in detail, using an Azure Mobile App as a beginning. You can
jump around at this point, but be aware that we expect you to cover these topics in order.
If you do the data chapter before covering authentication, it's likely you will have missed
important functionality in your app to complete the work.

[img1]: img/pic1.PNG
[img2]: img/xcode-add-appleid.PNG
[img3]: img/xcode-appleid.PNG
[img4]: img/xcode-signing-identities.PNG
[img5]: img/ios-final.PNG
[img6]: img/dataconns-sqldb.PNG
[img7]: img/dataconns-sqlsvr.PNG
[img8]: img/dataconns_success.PNG
[img9]: img/advanced-tools.PNG
[img10]: img/files-deployed.PNG
[img11]: img/change-target-framework.PNG
[img12]: img/xamarin-studio-new-client-project.PNG
[img13]: img/xamarin-studio-tasklist-project-layout.PNG
[img14]: img/xamarin-studio-manage-nuget-packages.PNG
[img15]: img/android-internet-permission.PNG
[img16]: img/android-emulator-app.PNG

[int-data]: ../chapter3/dataconcepts.md
[int-firstapp-pc]: ./firstapp_pc.md

[1]: https://azure.microsoft.com/en-us/documentation/learning-paths/appservice-mobileapps/
[2]: https://mockingbot.com/app/RQe0vlW0Hs8SchvHQ6d2W8995XNe8jK
[3]: https://mockingbot.com/
[4]: https://msdn.microsoft.com/en-us/data/ef
[5]: http://www.odata.org/documentation/odata-version-3-0/
[6]: https://portal.azure.com/
[7]: https://guidgenerator.com/
[8]: https://developer.xamarin.com/guides/xamarin-forms/creating-mobile-apps-xamarin-forms/
[9]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/blob/master/Chapter1
[10]: https://jfarrell.net/2015/02/07/platform-specific-styling-with-xamarin-forms/
[11]: https://developer.apple.com/
[12]: https://developer.apple.com/library/ios/documentation/IDEs/Conceptual/AppDistributionGuide/MaintainingCertificates/MaintainingCertificates.html#//apple_ref/doc/uid/TP40012582-CH31-SW6

