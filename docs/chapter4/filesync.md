# File Sync with Azure Mobile Apps

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

2. Ensure that your `Startup.MobileApp.cs` contains the `MapHttpAttributeRoutes()` step:

    ```csharp
    public static void ConfigureMobileApp(IAppBuilder app)
    {
        var config = new HttpConfiguration();

        // Register the StorageController routes
        config.MapHttpAttributeRoutes();

        new MobileAppConfiguration()
            .UseDefaultConfiguration()
            .ApplyTo(config);
    ```

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

<!-- Images -->

<!-- Links -->
[1]: https://github.com/Azure/azure-mobile-apps-net-files-client
[2]: http://www.nuget.org/packages/Microsoft.Azure.Mobile.Client.Files/1.0.0-beta-2
[3]: ./concepts.md#create-storage-acct
[4]: ../chapter3/projection.md
