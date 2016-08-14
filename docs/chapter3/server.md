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
will be added as well.

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
using System.Data.Entity.Spatial;
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

I've included several field types, including a complex type.  The basic requirement for a field is that it
must be serializable into a simple type in JSON.  However, complex types (that is, any type that can be
serialized to an object or array) may not be able to be handled and will always require special handling.

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

  You can now add a migration with the command `add-migration`.  The `add-migration`
command will request a name - it just has to be unique, but it's a good idea to make the name descriptive:

![][add-migration]

Once this is done, you can publish the project.  Right-click on the project and select **Publish...**.  Rather
than just clicking on the Publish button straight away (which is what we normally do), click on the **Settings**
tab.  Check the box to enable Code First Migrations:

![][publish-with-migrations]

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
    <httpRuntime targetFramework="4.5" />
    <compilation debug="true" targetFramework="4.6" />
    <customErrors mode="Off" />
  </system.web>
```

Then republish your project and the response from the server is much more informative.


<!-- Images -->
[new-controller-1]: img/new-controller-1.PNG
[new-controller-2]: img/new-controller-2.PNG
[enable-migrations]: img/enable-migrations.PNG
[add-migration]: img/add-migration.PNG
[publish-with-migrations]: img/publish-with-migrations.PNG


<!-- Links -->
[code-first migrations]: https://msdn.microsoft.com/en-us/data/jj591621