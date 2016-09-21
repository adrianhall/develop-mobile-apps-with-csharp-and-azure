# Dealing with Files

The most normal tasks for dealing with files are the upload and download of files to blob storage.  There is
a natural and consistent process to this which makes this recipe very repeatable.  First, deal with the things
you need before you start:

1. Create an Azure Storage Account and link it to your Azure App Service.
2. Decide how you want your files organized.
3. Create a WebAPI to generate a SAS token for your upload or download.

I've already discussed [how to create and link an Azure Storage Account][1].  Blob storage is organized in a
typical directory structure.  Each directory is called a container, and each file is a blob.  In the examples
for this section, I am going to store each uploaded file in a container based on the authenticated user.  My
WebAPI will create the appropriate container and then return an appropriate SAS token.

We haven't discussed custom code yet.  We will go much deeper than we do right now.  Custom APIs allow us to
write custom code and execute it within the context of the mobile backend.  It has access to many of the same
facilities as the rest of the mobile backend - things like app settings, connection strings, and the Entity
Framework structure.  To enable custom APIs, you need to alter the `App_Start\Startup.MobileApp.cs` file so
that the custom APIs are attached to HTTP routes properly:

```csharp
```

The Custom API is a standard ASP.NET controller with the `[MobileAppController]` attribute attached to the
class.  The `[MobileAppController]` signals to Azure Mobile Apps that the controller needs to be registered
under the `/api` endpoint.  It also handles API version checking (or at least checks that the `ZUMO-API-VERSION`
header is set to 2.0.0) and appropriately handles authorization if the `[Authorize]` attribute is present.

!!! info
    Ensure you install the latest version of the `WindowsAzure.Storage` Nuget package using the NuGet package
    Manager before continuing.

When we linked the Azure Storage account, we added a connection string called `MS_AzureStorageAccountConnectionString`.
This is included in the environment as `CUSTOMCONNSTR_MS_AzureStorageAccountConnectionString`.  We can set up
our custom API as follows:

```csharp
namespace Backend.Controllers
{
    [Authorize]
    [MobileappController]
    public class GetStorageTokenController : ApiController
    {
        private const string connString = "CUSTOMCONNSTR_MS_AzureStorageAccountConnectionString";

        public GetStorageTokenController()
        {
            ConnectionString = Environment.GetEnvironmentVariable(connString);
            StorageAccount = CloudStorageAccount.Parse(ConnectionString);
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        public string ConnectionString { get; }

        public CloudStorageAccount StorageAccount { get; }

        public CloudBlobClient BlobClient { get; }
    }
}
```

The `ConnectionString` property is the pointer to where the Azure Storage account is located and how to
access it.  the `StorageAccount` is a reference to that Azure Storage account.  Finally, the `BlobClient`
is an object used for accessing blob storage.  We can access any WebAPI methods in this class by using
the endpoint `/api/GetStorageToken` within our mobile client or using Postman.

Azure Storage doesn't have a true heirarchial container system.  It does have containers and directories
to organize things though, so we are going to use that:

## Uploading a File to Blob Storage

## Download a File from Blob Storage

<!-- Images -->

<!-- Links -->
[1]: ./concepts.md#create-storage-account
