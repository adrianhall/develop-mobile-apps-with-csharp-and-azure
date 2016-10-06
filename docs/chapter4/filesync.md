# File Sync with Azure Mobile Apps

!!! warn "Section In Progress"
    This section is a work in progress.  It is not complete and may contain errors.  Do not
    rely on the information in this section.

There are a couple of issues that the file management presented in the last section doesn't
handle.  The first issue is associated metadata.  If you are producing a picture album app, for
example, you will want to know what album the picture is in, how you have tagged the picture
and so on.  This associated metadata can be held in a table.  You do not have a direct
association between the file that has been uploaded and the metadata about the picture.
The second issue is offline sync.  The files are always retrieved from the server no matter
how many times you request them.  File-level caching is easy to implement but requires you
to maintain the list of files separately.  That brings us back to the associated metadata.

Azure Mobile Apps has a feature in the .NET library called _File Sync_.  Files are associated
with database records and synced alongside those database records.  The associated files can
be stored in anything (I recommend Azure Blob Storage, just like before) in any format and
any directory structure that you deem appropriate.  We will be producing a SAS token for
each file as it is synced.

!!! warn
    File Sync is in **PREVIEW** at this time and is available only for the .NET SDK.  You
    can follow the development at [the GitHub repository][1] for the SDK.  I used the [v1.0.0 beta-2][2]
    release of the Files SDK for this chapter.

Because Azure Mobile Apps File Sync support is in preview, there may be changes between the
release noted here and the final release.  There may also be bugs in the software.

Developing a File Sync solution is in two parts: a backend portion that generates the necessary
SAS tokens.  Most of the development is done on the mobile client side.  I am going to assume
that you have [connected an Azure Storage account][3] to your mobile backend during this walkthrough.

## Configuring the Mobile Backend

File Sync requires three additional HTTP endpoints.  The endpoints are all based on the record that the
files are associated with.  This is represented by the HTTP endpoint `/tables/{table}/{id}`, where `{table}`
is the name of the table and `{id}` is the Id field for the record.

* **POST /tables/{table}/{id}/StorageToken**

  When uploading a new file during file sync operations, we need to get a Storage Token to access
  that file.  This endpoint is called to generate the file.  It will look remarkably similar to the
  StorageToken endpoint we developed in the [last section].

* **GET /tables/{table}/{id}/MobileServiceFiles**

  We need to obtain a list of files to synchronize during the file sync process.

* **DELETE /tables/{table}/{id}/MobileServiceFiles/{filename}**

  When we delete a file from the cache on the mobile client, this endpoint is called to remove the file on the
  server during the sync operation.

When file are synchronized, the entire file is uploaded or downloaded directly to Azure Storage.  The
Azure Mobile Apps Files SDK for the server has default versions of these routines.  To implement them:

1. Add the **Microsoft.Azure.Mobile.Server.Files** NuGet package to your backend project.  You will need
   to use the _Include prerelease_ checkbox to find this package as it is still in preview.

2. Ensure that your `Startup.MobileApp.cs` contains the `config.MapHttpAttributeRoutes()` step.  If you
   have followed prior walkthroughs in this book, this will already be there.

3. Add a `StorageController` for each `TableController` that you use.

```csharp
[MobileAppController]
public class TodoItemStorageController : StorageController<TodoItem>
{
    [HttpPost]
    [Route("tables/TodoItem/{id}/StorageToken")]
    public async Task<HttpResponseMessage> StorageTokenAsync(string id, StorageTokenRequest value)
        => Request.CreateResponse(await GetStorageTokenAsync(id, value));

    [HttpGet]
    [Route("tables/TodoItem/{id}/MobileServiceFiles")]
    public async Task<HttpResponseMessage> GetFilesAsync(string id)
        => Request.CreateResponse(await GetRecordFilesAsync(id));

    [HttpDelete]
    [Route("tables/TodoItem/{id}/MobileServiceFiles/{name}")]
    public Task DeleteAsync(string id, string name) => base.DeleteFileAsync(id, name);
}
```

You must add a `StorageController` for each `TableController` that you are using.  The File Sync
process can provide sync capabilities for any file controller and it will attempt to sync any
associated files when used with the default settings.

Just as with the `TableController`, we can adjust the requests to handle per-user authorization.
The default version of `StorageController` does the same amount of authorization as the default
version of the `TableController` - none at all.  Our `TodoItemController` implements per-user
authorization.  We may want to model that in the storage controller as well.

Checking for a valid authorization token is the same.  Add an `[Authorize]` attribute to the
class or individual endpoints.

## Developing the Mobile Client

Implementing File Sync in a mobile client requires lots of moving parts.  First up, you need a few
additional NuGet packages.  We'll reuse the **Xam.Plugin.Media** plugin for picking a file to upload.
We will also need a cross-platform file storage package so that we can store the files in local memory
on the mobile device.   I'm going to use **PCLStorage** for iOS and Android, and **Windows.Storage**
for Windows platforms.  Finally, you will need the **Microsoft.Azure.Mobile.Client.Files** NuGet package.
This package holds the file sync logic.

!!! info
    The Microsoft.Azure.Mobile.Client.Files package is in pre-release right now.  You must check the
    **Include pre-release** box when looking for the NuGet package.

### Step 1: Add File Synchronization to the CloudService

The mobile client must register for file sync notifications and initialize the file sync capability
when initializing the sync store in `AzureCloudService.cs`:

```csharp
    private async Task InitializeAsync()
    {
        if (Client.SyncContext.IsInitialized)
        {
            return;
        }

        var store = new MobileServiceSQLiteStore(PlatformProvider.GetSyncStorePath());
        store.DefineTable<TodoItem>();

        // Initialize the File Sync Service
        Client.InitializeFileSyncContext(new FileSyncHandler(this), store);

        await Client.SyncContext.InitializeAsync(
            store,                              // The store to initalize
            new MobileServiceSyncHandler(),     // The table sync handler
            StoreTrackingOptions.NotifyLocalAndServerOperations
        );

        TaskTable = Client.GetSyncTable<TodoItem>();
    }
```

The **InitializeFileSyncContext()** method is an extension method in the **Microsoft.Azure.Mobile.Client.Files** package.
We need to define a file sync handler.  This is a class that implements the `IFileSyncHandler` interface.  It defines,
among other things, how we want to lay out the files on local storage.  It's generally implemented as a pair of classes -
the `IFileSyncHandler` is implemented in the shared PCL and a platform-specific provider is used to deal with the file
system itself.

The second part of the initialization is the `SyncContext.InitializeAsync()` call. We normally call this with just the
store and accept the defaults for all other parameters.  There are some additional parameters you can provider to the
**InitializeAsync()** method.  This method has [a signature][5] as follows:

```csharp
/// <summary>
/// Initializes the sync context.
/// </summary>
/// <param name="store">An instance of <see cref="IMobileServiceLocalStore"/>.</param>
/// <param name="handler">An instance of <see cref="IMobileServiceSyncHandler"/></param>
/// <param name="trackingOptions">A <see cref="StoreTrackingOptions"/> value indicating how store operations will be tracked, impacting which store events are raised.</param>
/// <returns>A task that completes when sync context has initialized.</returns>
Task InitializeAsync(IMobileServiceLocalStore store, IMobileServiceSyncHandler handler, StoreTrackingOptions trackingOptions);
```

The store is the only one that is required.  A default `MobileServiceSyncHandler` is initialized if you do not supply
one, and `StoreTrackingOptions.None` is used if you do not specify any tracking options.   When tracking options are
specified, an event manager is created and events are published to the event manager whenever a matching store change
is detected.  In this case, we've asked that an event manager be created for all store operations (both local and server
operations).  Our file sync handler will hook into the created event manager to detect changes to the store.

We also need to update our synchronization routine to also synchronize files:

```csharp
    public async Task SyncOfflineCacheAsync()
    {
        await InitializeAsync();

        if (Client.CurrentUser == null || Client.CurrentUser?.MobileServiceAuthenticationToken == null)
        {
            await LoginAsync();
        }

        await Client.SyncContext.PushAsync();
        await TaskTable.PushFileChangesAsync();
        await TaskTable.PullAsync("incsync_TodoItem", TaskTable.CreateQuery());
    }
```

The File Sync capabilities maintains an operations queue for uploading files from the mobile client.  The `PushFileChangesAsync()` method
is used to trigger the upload process.

!!! tip "Upload Files on Wifi only"
    One of the common requests is to detect the connection state.  You can use the [Xamarin Forms Labs Device][6] class
    to check if a site is reachable via Wifi.

### Step 2: Implement a PCL FileSyncHandler

When we initialized the file sync context, we provided a **FileSyncHandler** class. This class needs to implement the
**IFileSyncHandler** interface that is provided with the File Sync NuGet packages.  It is used by the file sync context
to effect the file sync process.  It contains two methods:

* **GetDataSource()** is called to get a data source that will be used to retrieve a file.
* **ProcessFileSynchronizationAction()** is called to effect a change in a file.

The latter method is used to download or delete a local file.  Let's take a look at `Services\FileSyncHandler.cs`:

```csharp
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.Services
{
    public class FileSyncHandler : IFileSyncHandler
    {
        private IFileSyncProvider fileProvider;
        private AzureCloudService cloudService;

        public FileSyncHandler(AzureCloudService cloudService)
        {
            fileProvider = DependencyService.Get<IFileSyncProvider>();
            this.cloudService = cloudService;
        }

        public Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
            => fileProvider.GetDataSourceAsync(metadata);

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete)
            {
                await fileProvider.DeleteLocalFileAsync(file);
            }
            else
            {
                if (file.TableName.ToLowerInvariant().Equals("todoitem"))
                {
                    await fileProvider.DownloadFileAsync<TodoItem>(file, cloudService.TaskTable);
                }
            }
        }
    }
}
```

I already mentioned that dealing with files is highly platform dependent.  This is very apparent when we deal with the file sync
handler.  We have to define a platform-specific service for retrieving a data source and deleting files.

### Step 3: Implement the Platform-Specific FileSyncProvider

We have mentioned the platform-specific `IFileSyncProvider` a few times.  We can define this interface in the shared project using
the calls we need to make:

```csharp
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Threading.Tasks;

namespace TaskList.Abstractions
{
    public interface IFileSyncProvider
    {
        Task DeleteLocalFileAsync(MobileServiceFile file);
        Task DownloadFileAsync<T>(MobileServiceFile file, IMobileServiceSyncTable<T> table);
        Task<IMobileServiceFileDataSource> GetDataSourceAsync(MobileServiceFileMetadata metadata);
    }
}
```

These three routines will keep the local file sync cache in sync with the server.  We will need to add some more to this later on when
we start adding new files to the file sync cache.  As always, I start with the Universal Windows platform.  As I mentioned at the
beginning, I'm going to use **Windows.Storage** for the file operations on Windows.

```csharp
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.IO;
using System.Threading.Tasks;
using TaskList.Abstractions;
using Windows.Storage;

[assembly: Xamarin.Forms.Dependency(typeof(TaskList.UWP.Services.UWPFileSyncProvider))]
namespace TaskList.UWP.Services
{
    public class UWPFileSyncProvider : IFileSyncProvider
    {
        /// <summary>
        /// Delete a file from the local file source
        /// </summary>
        /// <param name="file">The file to delete</param>
        public async Task DeleteLocalFileAsync(MobileServiceFile file)
        {
            var itemFolder = await GetLocalStorageAsync(file.TableName, file.ParentId);
            try
            {
                var storageFile = await itemFolder.GetFileAsync(file.Name);
                await storageFile.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
                // Ignore this error
            }
        }

        /// <summary>
        /// Download the specified file to local storage.
        /// </summary>
        /// <typeparam name="T">The type of the associated sync table</typeparam>
        /// <param name="file">The MobileServiceFile for the file to download</param>
        /// <param name="table">The associated local sync table reference</param>
        /// <returns></returns>
        public async Task DownloadFileAsync<T>(MobileServiceFile file, IMobileServiceSyncTable<T> table)
        {
            var itemFolder = await GetLocalStorageAsync(file.TableName, file.ParentId);
            await table.DownloadFileAsync(file, Path.Combine(itemFolder.Path, file.Name));
        }

        /// <summary>
        /// Obtains the data source for a local file request
        /// </summary>
        /// <param name="metadata">The metadata for the local file request</param>
        /// <returns>An IMobileServiceFileDataSource</returns>
        public async Task<IMobileServiceFileDataSource> GetDataSourceAsync(MobileServiceFileMetadata metadata)
        {
            var itemFolder = await GetLocalStorageAsync(metadata.ParentDataItemType, metadata.ParentDataItemId);
            return new PathMobileServiceFileDataSource(Path.Combine(itemFolder.Path, metadata.FileName));
        }

        /// <summary>
        /// Given a MobileServiceFile, locate the proper file path on the disk
        /// </summary>
        /// <param name="file">The MobileServiceFile</param>
        /// <returns>The path to the MobileServiceFile</returns>
        private async Task<StorageFolder> GetLocalStorageAsync(string tablename, string id)
        {
            var baseFolder = ApplicationData.Current.LocalCacheFolder;

            // Find the folder for the table, creating it if it does not exist
            var tableFolder = await baseFolder.CreateFolderAsync(tablename, CreationCollisionOption.OpenIfExists);

            // Find the folder for the item, creating it if it does not exist, and return it
            return await tableFolder.CreateFolderAsync(id, CreationCollisionOption.OpenIfExists);
        }
    }
}
```

The **Windows.Storage** subsystem does most of the work for us.  The private **GetLocalStorageAsync()** method returns
a reference to the **StorageFolder** for a specific item in a table, creating any folders that are required.  We use the
**LocalCacheFolder** as a base and then lay out the files as `{table}\{id}\{filename}` under the local cache folder.  We
are using the local cache folder instead of the more normal **LocalFolder** because we don't need to backup the files.  The
LocalFolder (which is a folder under the users `AppData` directory and organized by application) is backed up when a backup
is requested.  The LocalCacheFolder (which is in the same place) is explicitly excluded from backups normally.

The code for Android and iOS may be completely different, but the concepts are the same.  Let's look at the Android version:

```csharp
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using PCLStorage;
using PCLStorage.Exceptions;
using System;
using System.Threading.Tasks;
using TaskList.Abstractions;

[assembly: Xamarin.Forms.Dependency(typeof(TaskList.Droid.Services.DroidFileSyncProvider))]
namespace TaskList.Droid.Services
{
    public class DroidFileSyncProvider : IFileSyncProvider
    {
        /// <summary>
        /// Delete a file from the local file source
        /// </summary>
        /// <param name="file">The file to delete</param>
        public async Task DeleteLocalFileAsync(MobileServiceFile file)
        {
            var itemFolder = await GetLocalStorageAsync(file.TableName, file.ParentId);
            try
            {
                var item = await itemFolder.GetFileAsync(file.Name);
                await item.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
                // Ignore this error
            }
        }

        /// <summary>
        /// Download the specified file to local storage.
        /// </summary>
        /// <typeparam name="T">The type of the associated sync table</typeparam>
        /// <param name="file">The MobileServiceFile for the file to download</param>
        /// <param name="table">The associated local sync table reference</param>
        public async Task DownloadFileAsync<T>(MobileServiceFile file, IMobileServiceSyncTable<T> table)
        {
            var itemFolder = await GetLocalStorageAsync(file.TableName, file.ParentId);
            await table.DownloadFileAsync(file, PortablePath.Combine(itemFolder.Path, file.Name));
        }

        /// <summary>
        /// Obtains the data source for a local file request
        /// </summary>
        /// <param name="metadata">The metadata for the local file request</param>
        /// <returns>An IMobileServiceFileDataSource</returns>
        public async Task<IMobileServiceFileDataSource> GetDataSourceAsync(MobileServiceFileMetadata metadata)
        {
            var itemFolder = await GetLocalStorageAsync(metadata.ParentDataItemType, metadata.ParentDataItemId);
            return new PathMobileServiceFileDataSource(PortablePath.Combine(itemFolder.Path, metadata.FileName));
        }

        /// <summary>
        /// Given a MobileServiceFile, locate the proper file path on the disk
        /// </summary>
        /// <param name="file">The MobileServiceFile</param>
        /// <returns>The path to the MobileServiceFile</returns>
        private async Task<IFolder> GetLocalStorageAsync(string tablename, string id)
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var baseFolder = await FileSystem.Current.LocalStorage.GetFolderAsync(basePath);

            // Find the folder for the table, creating it if it does not exist
            var tableFolder = await baseFolder.CreateFolderAsync(tablename, CreationCollisionOption.OpenIfExists);

            // Find the folder for the item, creating it if it does not exist, and return it
            return await tableFolder.CreateFolderAsync(id, CreationCollisionOption.OpenIfExists);
        }
    }
}
```

You may find this shockingly similar to the Universal Windows version.  That's because it is.  **PCLStorage** is a cross-platform
way that is very similar to **Windows.Storage** - enough that the exact same methods are used, albeit in a different namespace and
belonging to a different class.  We still need to account for certain differences:

* `PortablePath.Combine` is used instead of `Path.Combine`.  PortablePath is a drop-in replacement that comes with PCLStorage.
* We use `IFolder` instead of `StorageFolder` - the concepts are the same.
* The starting path for our storage is different.  In this case, I'm using the `ApplicationData` location, which is defined
  as `$HOME/.config`.  `$HOME` is always '/data/data/@PACKAGE_NAME@' on Android devices.  The package name is defined in the
  application manifest.

We have written a lot of code.  Right now, the mobile client should be able to be built and run on all platforms.  Thus far, it
only downloads files and maintains the cache.

### Step 4: Add File Upload

The piece we are missing from our file sync use case on the mobile client is the addition of UI and backing code for the upload
of a file.  As with file upload and download, this is very specific to a particular use case.  In this case, I'm going to assume
that we want to attach pictures to a database record.  We'll do this by adding an "Add File" button to our task bar in the Task
Details page that calls a command to do the upload.

!!! note
    I've already done the work of coding the button for the File Upload on the Task Details Page before starting.

### Step 5: Update the UI to handle the File List



## Handling Authorization in File Sync

When we looked at the [data sync capabilities][4], we used the concepts of filters and transforms to
ensure that a user could only see and change the data for which they had permissions.  Similar concepts
can be applied to the file sync capabilities as well.  There are three HTTP endpoints to concern
ourselves with:

* The `POST` endpoint (for obtaining a SAS token) should only produce an appropriate token based on
  the permissions structure.  If the user does not have the permissions for the requested file
  operations, a **403 Forbidden** response should be returned.

* The `GET` endpoint (for obtaining a list of files) should only produce a list of files if the user
  is allowed to see the files.  If the user cannot see the associated database record, then a **404 Not Found**
  response should be returned.  If the user can see the records but is not allowed to view the files,
  an empty set should be returned.

* The `DELETE` endpoint (for deleting a file) should only delete the file if the user is allowed to
  delete files.  If the user does not have permission to delete files, a **403 Forbidden** should be
  returned.

We can use these rules to produce a constrained storage table controller.  For example, the following
code could be used for a table that did not allow file sync:

```csharp
    [MobileAppController]
    public class TagStorageController : StorageController<Tag>
    {
        [HttpPost]
        [Route("tables/TodoItem/{id}/StorageToken")]
        public HttpResponseMessage StorageToken(string id, StorageTokenRequest value)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        [HttpGet]
        [Route("tables/TodoItem/{id}/MobileServiceFiles")]
        public HttpResponseMessage GetFiles(string id)
            => Request.CreateResponse(new List<MobileServiceFile>());

        [HttpDelete]
        [Route("tables/TodoItem/{id}/MobileServiceFiles/{name}")]
        public Task DeleteAsync(string id, string name)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }
    }
```

Since every single table controller needs an associated storage controller, you can use this pattern for the
storage controllers that should not support file sync.

One of the common patterns in database access is the _per-user table_.  We covered the per-user table pattern
when we discussed [filters and transforms][4].  It is only natural that you would want to extend this to the
file sync capabilities as well.  The following code will limit permissions to the owning user:

```csharp
    [MobileAppController]
    public class TagStorageController : StorageController<Tag>
    {
        EntityDomainManager<TodoItem> domainManager;

        public TodoItemStorageController()
        {
            MobileServiceContext context = new MobileServiceContext();
            domainManager = new EntityDomainManager<TodoItem>(context, Request, enableSoftDelete: true);
        }

        [Route("tables/TodoItem/{id}/StorageToken")]
        public async Task<HttpResponseMessage> StorageTokenAsync(string id, StorageTokenRequest value)
        {
            if (!await IsRecordOwner(id))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            return Request.CreateResponse(await GetStorageTokenAsync(id, value));
        }

        [HttpGet]
        [Route("tables/TodoItem/{id}/MobileServiceFiles")]
        public async Task<HttpResponseMessage> GetFilesAsync(string id)
        {
            if (!await IsRecordOwner(id))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            return Request.CreateResponse(await GetRecordFilesAsync(id));
        }

        [HttpDelete]
        [Route("tables/TodoItem/{id}/MobileServiceFiles/{name}")]
        public async Task DeleteAsync(string id, string name)
        {
            if (!await IsRecordOwner(id))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            await base.DeleteFileAsync(id, name);
        }

        private async Task<bool> IsRecordOwner(string id)
        {
            var principal = this.User as ClaimsPrincipal;
            var sid = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var item = (await domainManager.LookupAsync(id)).Queryable.FirstOrDefault();
            return item.UserId.Equals(sid);
        }
    }
```

If you try to access the file set for the records that you don't own, then we return a **403 Forbidden** response.  We can
get more complicated than this.  The **StorageTokenRequest** is an object that contains details about the file being requested
and the permissions being sought.  This allows us to provide fine-grained controls on what is allowed.

In your mobile client, you must handle any errors that you are producing from your service.  In this case, for example, we
need to handle the **403 Forbidden** response - something that is not normally produced.

<!-- Images -->

<!-- Links -->
[1]: https://github.com/Azure/azure-mobile-apps-net-files-client
[2]: http://www.nuget.org/packages/Microsoft.Azure.Mobile.Client.Files/1.0.0-beta-2
[3]: ./concepts.md#create-storage-acct
[4]: ../chapter3/projection.md
[5]: https://github.com/Azure/azure-mobile-apps-net-client/blob/fd559611a7af8bfb5aa9d51ba30ad3b48b80a832/src/Microsoft.WindowsAzure.MobileServices/Table/Sync/IMobileServiceSyncContext.cs#L43
[6]: https://github.com/XLabs/Xamarin-Forms-Labs/wiki/Device
