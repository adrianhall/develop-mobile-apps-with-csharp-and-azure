# Azure Storage Concepts

When dealing with cloud concepts, there are multiple operating levels one can think about.  At the bottom layer
is _Infrastructure as a Service_.  Most people think of this as the Virtual Machine layer, but it also incorporates
basic networking and storage concepts.  As you move to higher level services, you gain a lot of efficiencies by
adding software components, you lose a lot of the potential management headaches, but you also lose flexibility
in what you can do to the platform.  At the top of the stack is _Software as a Service_.  You may be running
a helpdesk, for example, but you are completely isolated from what operating system is being run, what web services
are being run, APIs that can be accessed and language that is used.

Azure Mobile Apps is an opinionated combination of a client and server SDK running on top of a standard ASP.NET
based web service and is normally thought of as being a _Platform as a Service_.  You get to choose what database
to use, what tables to expose, and what programming language to use.  You don't get to determine when the operating
system is patched or what patches are applied.  It's a middle of the road between SaaS and IaaS.

That isn't to say we can't dip down sometimes to deal with lower level cloud services, nor to access higher level
SaaS APIs.  One of those times is when dealing with files.  Storage is conceptually easy - you have an amount of
disk and you can store files on it.  However, the management of that storage is complicated.  Placing that storage
at the service of a scalable web application is similarly complicated.  What we intend to do is develop a set of
skills that make developing storage based mobile applications easy.

## Blobs, Table, Queues and files

At the top of my list of "storage made complicated" is the cloud storage concepts.  In the old days, we stored files
on a file system and we didn't really have to worry about differing types of storage, redundancy and capabilities.
Cloud storage tends to come in multiple flavors:

* The base storage type is **Blob Storage**.  Put simply, you have containers (roughly analogous to directories) and
blobs (roughly analogous to files).  It's the cheapest form of storage and is used for many things, including the underlying
storage for virtual machine disks.  Blob storage has many advantages.  From a mobile perspective, developers will
appreciate the upload/download restart capabilities within the SDK.

* We've already introduced **Table Storage** in [the last chapter][ch3-1].  It is more analogous to a NoSQL store for storing
key / attribute values.  It has a schemaless design, so you can store basic JSON objects.  However, it has limited
query capabilities, as we discussed in the last chapter.  That makes it unsuited to large scale query-driven applications.

* You may think you want **Files Storage**.  This provides an SMB interface to the storage layer.  You would use Files
Storage if you want to browse files from your PC or Mac as you can mount the file system directly from Azure Storage.

* Finally, **Queue Storage** provides cloud messaging between application components.  We'll get onto Azure Functions
later on, during our look at Custom API.  Queue Storage will definitely be coming into play then.  Think of Queue
Storage as the glue that ties work flow components together.

The real question is when should you use File Storage and when should you use Blob Storage.  For more applications,
Blob Storage is going to save you money over File Storage, so it's pretty much always the better choice.  You should
only be thinking of File Storage if you have other components of your system that need to access the data you upload
that can only access that data via an SMB interface.

If you need to explore the data that you upload or download, you can use the [Azure Storage Explorer][1] as a standalone
application or you can use the Cloud Explorer in [Visual Studio][2].

## <a name="create-storage-acct"></a>Creating and Linking a Storage Account

Before we can use storage, we need to set up a storage account and connect it to our environment.  This involves:

1. Create a Resource Group
2. Create an Azure App Service
3. Set up authentication on the Azure App Service
4. Create a Storage Account
5. Link the Storage Account to the Azure App Service.

We've already covered the first three items in previous chapters.  We've also created a storage account and linked it
to the mobile backend during our look at the [Storage Domain Manager][ch3-1].  To create a Storage Account:

* Log on to the [Azure portal].
* Click the big **+ NEW** button in the top left corner.
* Click **Data + Storage**, then **Storage account**.
* Fill in the form:
    * The name can only contain letters and numbers and must be unique.  A GUID without the dashes is a good choice.
    * The **Deployment model** should be set to **Resource manager**.
    * The **Account kind** should be set to **General purpose**.
    * The **Performance** should be set to **Standard** for this example.
    * The **Replication** should be set to **Locally-redundant storage (LRS)**.
    * Set the **Resource group** to your existing resource group.
    * Set the **Location** to the same location as your App Service.
* Click **Create**.

Just like SQL Azure, Azure Storage has some great scalability and redundancy features if your backend takes advantage of them.
For example, you have the option of **Premium Storage** - this provides all-SSD storage that has a large IOPS performance
number.  You can also decide how redundant you want the storage.  Azure always keeps 3 copies of your data.  You can choose
to increase the number of copies and decide whether the additional copies will be in the same datacenter, another datacenter
in the same region or another region.  We have selected the slowest performance and least redundant options here to keep the
cost down on your service.

!!! warn
    There is no "free" option for Azure Storage.  You pay by the kilobyte depending on the performance and redundancy selected.

Once the Azure Storage account is deployed, you can link the storage account to your App Service:

* Open your App Service in the [Azure portal].
* Click  **Data Connections** under the **MOBILE** section in the settings menu.
* Click **+ ADD**
* In the **Add data connection** blade:
    * Set the Type to **Storage**.
    * Click the **Storage** link.
    * In the **Storage Account** selector, click the storage account you just created.
    * Click the **Connection string**.
    * In the **Connection string** selector, make a note of the **Name** field.
    * Click **OK**.
    * Click **OK** to close the **Add data connection** blade.

Click on the **Application Settings** menu option, then scroll down to the **Connection Strings** section.  Note that the portal
has created the connection string as an App Setting for you with the right value:

```bash
DefaultEndpointsProtocol=https;AccountName=thebook;AccountKey=<key1>
```

By default, the connection string is called `MS_AzureStorageAccountConnectionString` and we will use that throughout our
examples.

The key is the access key for the storage.  When a storage account is created, two keys are also created.  The keys are used for
secure access to the storage area.  You should never distribute the storage keys nor check them into source control.  If you feel
they have been compromised, you should regenerate them.  There are two keys for this purpose.  The process of regeneration is:

1. Regenerate KEY2
2. Place the regenerated KEY2 in the connection string and restart your App Service.
3. Regenerate key1
4. Place the regenerated KEY1 in the connection string and restart your App Service.

In this way, your App Service will always be using KEY1 except during regeneration.  You can avoid the restart of your App Service
by providing a management interface that sets the Account Key for the App Service.

!!! tip
    For local development, there is the [Azure Storage Emulator][3].  The connection string when using the Azure Storage
    Emulator is `UseDevelopmentStorage=true`.

It's normal to add the storage connection string to the `Web.config` file with the following:

```xml
<connectionStrings>
    <add name="MS_AzureStorageAccountConnectionString" connectionString="UseDevelopmentStorage=true" />
</connectionStrings>
```

This will be overwritten by the connection string in the App Service Application Settings.  Effectively, you will be using the
Azure Storage Emulator during local development and Azure Storage when you deploy to Azure App Service.

## The Shared Access Signature (SAS)

The storage account key is kind of like the root or Administrator password.  You should always protect it, never send it to a
third party and regenerate it on a regular basis.  You avoid storing the storage account key in source code by linking the
storage account to the App Service.  The key is stored in the connection string instead.  You should never ship an account
key to your mobile account.

The Azure Storage SDK already has many of the features that you want in handling file upload and download.  Azure Storage is
optimized for streaming, for example.  You can upload or download blobs in blocks, allowing you to restart the transfer and
provide feedback to the user on progress, for example.   You will inevitably be drawn to having your mobile client interact
with Azure Storage directly rather than having an intermediary web service for this reason.

If you want to interact with Azure Storage directly and you shouldn't give out the account key, how do you deal with the
security of the service?  The answer is with a Shared Access Signature, or SAS.  The **Service SAS** delegates access
to just a single resource in one of the storage services (Blob, Table, Queue or File service).

!!! info
    There is also an [Account SAS][4] which delegates access to resources in more than one service.  You generally don't
    want this in application development.

A service SAS is a URI that is used when accessing the resource.  It consists of the URI to the resource followed by a
SAS token.  The SAS token is an cryptographically signed opaque token that the storage service decodes.  Minimally, it
provides an expiry time and the permissions being granted to the SAS.

!!! warn
    A SAS token **ALWAYS** expires.  There is no way to produce a permanent SAS token.  If you think you need one,
    think again.  In mobile development, you **NEVER** want a non-expiring token.

Accessing Azure Storage is always done with a specific [version of the REST API][5] and that follows through to the SDK.  You
should always request a SAS token for the appropriate API you are going to be using.   We'll cover the various
methods of obtaining a SAS later in the chapter.

## File Sync with Azure Mobile Apps

It's actually fairly rare that you will want to upload and download random files. Generally, your application will need
to associate your files with a database record.  Take, for example, the common patterns for image management.  Your
users will want to organize the images into albums and also store metadata about the images.  They will want to
search for images based on the metadata.  The same can be said for many other applications.  Inevitably, you will
want to associate your files with database records.

Our database records are available offline via the Azure Mobile Apps offline sync capability.  It is only natural,
therefore, that we will want to have our pictures available offline as well.  Fortunately, the mobile clients all
have the capability of organizing files on the device, and Azure Mobile Apps has the capability of synchronizing
files between Azure Storage and the device.  No extra data is stored with the database record.  However, it's
important that the data within Azure Storage is organized properly.

In this chapter, we will again be working with Azure Mobile Apps and our typical Task List application.  We will add
the capability to upload and download files both at an application level and a per-record basis with offline sync.
Before you continue:

* Create a new resource group
* Create an App Service with a SQL Azure database
* Configure the App Service with some sort of authentication
* Create a Storage Account
* Link the Storage Account to the App Service
* Deploy the code from the Backend project for the [Chapter4 solution].
* Ensure that the Tasklist application runs for your platform.

We will use this as a starting point for our investigations.  You can download the starter project from the books
[GitHub repository][5].


<!-- Links -->
[ch3-1]: ../chapter3/domainmgr.md#storage-domain-mgr
[1]: https://azure.microsoft.com/documentation/articles/vs-azure-tools-storage-manage-with-storage-explorer/
[2]: https://visualstudiogallery.msdn.microsoft.com/84e83a7c-9606-4f9f-83dd-0f6182f13add
[3]: https://azure.microsoft.com/en-us/documentation/articles/storage-use-emulator/
[4]: https://msdn.microsoft.com/en-us/library/mt584140.aspx
[5]: https://github.com/adrianhall/develop-mobile-apps-with-csharp-and-azure/tree/chapter4-start
