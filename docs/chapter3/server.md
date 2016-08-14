# Implementing Table Controllers

The central component for providing a table endpoint on the Azure App Service side of things occurs in your
backend project.  You must implement a Table Controller.  This is a specialized version of an ApiController
that has some similarities with an ODataController.  However, it has its own base class and the Azure Mobile
Apps SDK simplifies the process of making them.

## Implementing Code First Migrations

Before we can get started with adding another table controller, we have to deal with modifications to our
database schema.  The default code for Azure Mobile Apps will deploy the configured schema **only if the
database is empty**.  If the database is not empty, you have to do extra work.

Nothing causes more headaches in an Azure Mobile Apps backend than [code-first migrations].  A code-first
migration is simply a set of configuration commands that updates the database to support the new
database model.  If you try to publish this application, you will see an `InvalidOperationException`
and your service will likely crash.  If you manage to trap the error, it will say _The model backing
the 'MobileServiceContext' context has changed since the database was created. Consider using Code First
Migrations to update the database._  That's fairly specific and is common to all applications based
on Entity Framework that use code-first models.

> There is an alternative here called **Database First Models**.  In this alternative, you create the
database first then create the models to match.  However, Azure Mobile Apps requires specific configuration
of mobile tables that you will need to take care of.  See the section on using existing SQL tables later
on for details.

The first step is to enable migrations.  Go to **View** -> **Other Windows** -> **Package Manager Console**.
This window will generally appear in the same place as your Output and Error List windows at the bottom
of the screen.  Type `enable-migrations` in it:

![][enable-migrations]

This creates a `Migrations` folder to hold the code-first migrations.  An initial `Configuration.cs` object
will be added as well.  We also need to apply an Initial migration.  We can do this with the command
`add-migration Initial`.

![][add-initial-migration]

This will add a few files into the `Migrations` folder that represent the current state of affairs for
the database.

Code First Migrations can be applied manually or automatically.  I personally prefer the automatic method.
To implement automated Code First Migrations, edit the `App_Start\Startup.MobileApp.cs` file:

```csharp
public static void ConfigureMobileApp(IAppBuilder app)
{
    var httpConfig = new HttpConfiguration();
    var mobileConfig = new MobileAppConfiguration();

    mobileConfig
        .AddTablesWithEntityFramework()
        .ApplyTo(httpConfig);

    // Automatic Code First Migrations
    var migrator = new DbMigrator(new Migrations.Configuration());
    migrator.Update();

    app.UseWebApi(httpConfig);
}
```

We have replaced the `DbInitializer()` (which was the method that created the database for us) with the
automatic database migrator code.

There is one issue that will cause some problems.  We are no longer using a database initializer.  This
means that the special system columns will no longer be wired up to update their values automatically.
We can fix that by configuring the SqlGenerator in `Migrations\Configuration.cs`:

```csharp
public Configuration()
{
    AutomaticMigrationsEnabled = false;
    SetSqlGenerator("System.Data.SqlClient", new EntityTableSqlGenerator());
}
```

Since we are not using a database initializer, our seed data has also gone.  You may as well delete the
`MobileServiceInitializer` class in the `App_Start\Startup.MobileApp.cs` as it isn't doing anything
any more.  You can move the seed data to the `Migrations\Configuration.cs` file though:

```csharp
namespace Chapter3.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using DataObjects;
    using Microsoft.Azure.Mobile.Server.Tables;

    internal sealed class Configuration : DbMigrationsConfiguration<Chapter3.Models.MobileServiceContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            SetSqlGenerator("System.Data.SqlClient", new EntityTableSqlGenerator());
        }

        protected override void Seed(Chapter3.Models.MobileServiceContext context)
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

The contents of the `Seed` method are a cut-and-paste from the `MobileServiceInitializer` version.

If all goes well, you can clear your database (or delete it and re-create it), then publish this project
and do a GET of the `/tables/todoitem` endpoint.  You should still see your data.  You should do a little
more investigation however.

* Open the database in the **SQL Server Object Explorer**.
* Expand the **Tables** node.
* Right-click on the **dbo.__MigrationHistory** table and select **View Data**.

![][initial-migration-applied]

There should be one row with a name that indicates it is the initial migration.

The final step is to apply the migration to the local system.  In the Package Manager Console, enter
the command `update-database` to apply the existing migration.

## Adding a SQL Table Controller

Before we can use a table controller, we need to add one.  This has three steps:

* Create a Data Transfer Object (DTO)
* Create a Table Controller
* Create a Code-First Migration

### Creating a Data Transfer Object

A Data Transfer Object (or DTO as it is commonly known) is the wire representation of the model for your
table.  It must inherit from a concrete implementation of `ITableData`.  The Azure Mobile Apps SDK includes
`EntityData` for this reason.  EntityData is a concrete implementation that works with Entity Framework.

> You can't just assume EntityData will work with other data stores.  There are Entity Framework specific
attributes decorating the properties for EntityData that will likely be different for other stores.

The default Azure Mobile Apps project that is supplied with the Azure SDK provides a folder for storing
DTOs called `DataObjects`.  Let's create a DTO by right-clicking on the **DataObjects** folder, then
using **Add** -> **Class...**:

```csharp
using System;
using Microsoft.Azure.Mobile.Server;

namespace Chapter3.DataObjects
{
    public class Example : EntityData
    {
        public string StringField { get; set; }
        public int IntField { get; set; }
        public double DoubleField { get; set; }
        public DateTimeOffset DateTimeField { get; set; }
    }
}
```

> Don't call your model `SomethingDTO`.  This ends up as a `/tables/somethingDTO` endpoint and a `SomethingDTO`
table in your database.  Just call it `Something`.  All the names will then line up properly.

I've included several field types, including a complex type.  The basic requirement for a field is that it
must be serialized into a simple JSON type during transfer between the server and the mobile client.  Complex
types (that is, any type that can be serialized to an object or array) will always require special handling
and may not be able to be used at all.

### Create a Table Controller

Visual Studio with the Azure SDK provides some help in creating a table controller.  Right-click on the
**Controllers** node and select **Add** -> **Controller...**.

![][new-controller-1]

The Azure SDK provides scaffolding for a new table controller.  Select it and then click on **Add**.

![][new-controller-2]

The dialog asks for the model (which is actually a DTO) and the data context (which is already created).
Once you select the model, the controller name is created for you.  You can change it if you like, but
it's common practice to not do this.

Once the scaffolding is finished, you can look at your newly created table controller.  We do want to
do one change.  We want to enable soft delete so that our table controller supports offline sync
scenarios properly.  To do this, go into the `Initialize()` method and change the constructor of the
`EntityDomainManager`:

```csharp
protected override void Initialize(HttpControllerContext controllerContext)
{
    base.Initialize(controllerContext);
    MobileServiceContext context = new MobileServiceContext();
    DomainManager = new EntityDomainManager<Example>(context, Request, enableSoftDelete: true);
}
```

### Creating a Code-First Migration

You must add a code first migration to update the database when it is published. Use the `add-migration`
command in the Package Manager Console.  The `add-migration` command will request a name - it just has
to be unique, but it's a good idea to make the name descriptive:

![][add-migration]

You should also use `update-database` to apply the change to the local database (if any):

![][update-database]

Once this is done, you can publish the project.  Right-click on the project and select **Publish...**.  Once
the project is published, you should be able to send a query to the `/tables/example` endpoint using Postman
and get an empty array.  You should also be able to insert, update and delete entities as you can with the
`TodoItem` table.

### Handling Publish Failures

Sometimes, the publish fails.  It seems that whenever I start with code-first migrations, my publish fails.
I get a nice error screen, but no actual error.  At least half the time, the problem is not my code-first
migration, but something else.  For instance, one of the things I tend to do is update my NuGet packages.
This inevitably breaks something.

Fortunately, once the error message is known, it's generally trivial to correct the error.  You can turn
custom error messages off (and thus expose the original error message) by editing the Web.config file.
Locate the `<system.web>` section and add the `<customErrors mode="Off"/>` line:

```xml
  <system.web>
    <httpRuntime targetFramework="4.6" />
    <compilation debug="true" targetFramework="4.6" />
    <customErrors mode="Off" />
  </system.web>
```

Then republish your project and the response from the server is much more informative.

###  Turning on Diagnostic Logs

You can log all the SQL statements that Entity Framework executes on your behalf by adding a Database
Log.  Edit the `Models\MobileServiceContext.cs` file:

```csharp
public class MobileServiceContext : DbContext
{
    private const string connectionStringName = "Name=MS_TableConnectionString";

    public MobileServiceContext() : base(connectionStringName)
    {
        Database.Log = s => WriteLog(s);
    }

    public void WriteLog(string msg)
    {
        System.Diagnostics.Debug.WriteLine(msg);
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Conventions.Add(
            new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
    }

    public DbSet<DataObjects.TodoItem> TodoItems { get; set; }
    public DbSet<DataObjects.Example> Examples { get; set; }
}
```

You have to use a real method.  System.Diagnostics.Debug is removed from the context when DEBUG is not
defined, so you can't just use it directly.   Using an interim method works around that problem. Azure App
Service captures the output from the console and places it into the log viewer for you.

To turn on diagnostic logging:

* Log in to the [Azure Portal].
* Click on **App Services** then your App Service.
* Find **Diagnostic Logs** in the list of settings (you can use the search box).
* Turn on **Application Logging (Filesystem)** with a level of **Verbose**.
* Click on **Save**.

To view the diagnostic logs in the portal, find **Log Stream** in the list of settings (again, you can
use the search box).  You can also get the diagnostic logs within Visual Studio.

* Open the **Server Explorer**.
* Expand **Azure**, **App Service**, your resource group.
* Right-click on the your App Service and select **View Streaming Logs**.

![][debug-logging]

## Using an existing SQL Table

<!-- Images -->
[new-controller-1]: img/new-controller-1.PNG
[new-controller-2]: img/new-controller-2.PNG
[enable-migrations]: img/enable-migrations.PNG
[add-migration]: img/add-migration.PNG
[add-initial-migration]: img/add-initial-migration.PNG
[initial-migration-applied]: img/initial-migration-applied.PNG
[update-database]: img/update-database.PNG
[debug-logging]: img/debug-logging.PNG

<!-- Links -->
[code-first migrations]: https://msdn.microsoft.com/en-us/data/jj591621
[Azure Portal]: https://portal.azure.com
