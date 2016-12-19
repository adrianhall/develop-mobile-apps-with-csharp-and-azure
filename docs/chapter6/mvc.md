At some point, you will likely want to pair your mobile application with a web interface.  This may be because you have a simplified
mobile app whereas you may have a more fully featured app within the web.  For example,  I see this design featured prominently in
fitness apps.  The mobile app is a news feed and recording device, whereas the web interface contains all the fitness analytics.  You
may also have some sort of administrative interface that provides an alternate view of the data.

Whatever the reason you decide to support web and mobile together, you will need to convert your Azure Mobile App backend to a fully-fledged
ASP.NET MVC application.  Fortunately, the process of merging Azure Mobile Apps with an existing ASP.NET MVC application is simple.  Doing
the reverse (merging MVC into Azure Mobile Apps) is considerably more complex.

Start by creating a new ASP.NET application with File -> New Project.  Select the **ASP.NET Web Application (.NET Framework)** project
template.  Then select th **MVC** template.  Change the Authentication to **No Authentication**.

![][img1]

Click **OK** to create the project.  Run your project to ensure it is working correctly.

!!! info "Why is merging MVC into Azure Mobile Apps so hard?"
    ASP.NET requires a large number of NuGet packages to implement MVC.  These are provided for you when you start from the
    appropriate template, but you will need to add them yourself when you start from the Azure Mobile Apps template.

Now that you have an MVC project, let's add Azure Mobile Apps to it.  Start by adding the **Microsoft.Azure.Mobile.Server.Quickstart**
NuGet package.  This NuGet package contains dependencies for all the other Azure Mobile Apps SDK requirements.  If you want the
big long list instead, add the following:

*  AutoMapper
*  EntityFramework
*  Microsoft.AspNet.WebApi.Client
*  Microsoft.AspNet.WebApi.Core
*  Microsoft.AspNet.WebApi.Owin
*  Microsoft.Azure.Mobile.Server
*  Microsoft.Azure.Mobile.Server.Authentication
*  Microsoft.Azure.Mobile.Server.Notifications
*  Microsoft.Azure.NotificationHubs
*  Microsoft.Data.Edm
*  Microsoft.Owin
*  Microsoft.Owin.Security
*  Microsoft.WindowsAzure.ConfigurationManager
*  Owin
*  System.IdentityModel.Tokens.Jwt
*  System.Spatial

Using the Quickstart package is a serious time saver over having to type in 16 package names.

!!! tip "Upgrading to .NET 4.6"
    If you want to run your ASP.NET service under .NET Framework 4.6, you can upgrade just about everything.  However, the
    `System.IdentityModel.Tokens.Jwt` package should not be upgraded - leave it on the latest v4.x release.  In addition, do
    not upgrade `AutoMapper` beyond v3.3.1 if you are using the `MappedEntityDomainManager` class.

You can then copy the the following files from your original Azure Mobile Apps server project to the new project.

*  `App_Start\Startup.MobileApp.cs`
*  `Controllers\*.cs`
*  `DataObjects\*.cs`
*  `Models\MobileServiceContext.cs`

Finally, adjust or create the `Startup.cs` file as follows:

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

Note the addition of the `ConfigureMobileApp()` call.  If you are starting from the suggested template, this file does not exist and you
will need to create it.

## Sharing the Database

Underneath the covers, Azure Mobile Apps uses [EntityFramework][1] to access the database.  It requires certain adjustments to the models,
as we discussed in [Chapter 3].  However, you can still use the same Entity Framework context to access the database.  There are some
caveats that must be followed, however:

*  Inserts must set the fields that are not managed by the database (such as `Id`).
*  Deletes must set the `Deleted` column if using Soft Delete, instead of directly deleting records.

As an example, let's create a default view for handling our TodoItem controller.  In MVC, you need a Model, View and Controller class.  The
Model will be handled by our existing `DataObjects\TodoItem.cs` class.  The Controller and View will be new classes.

## Sharing Authentication

<!-- Images -->
[img1]: img/new-project.png

<!-- Links -->
