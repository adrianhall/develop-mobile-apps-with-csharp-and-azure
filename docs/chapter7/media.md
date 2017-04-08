# Azure Media Services

One of the common use cases for mobile applications involves video streaming.  In the consumer space, this can include applications like Hulu or Netflix along with video reviews and new segments in apps like CNN and CNET.  In the enterprise space, we see video learning and employee broadcast solutions.

Whatever their source, they have some basic functionality in common:

* The video asset is uploaded and converted (also known as encoding) to a streaming format.
* A live video channel can be provided for multiplex streaming to a large audience.
* The encoded video is provided to clients for download with a suitable web endpoint.
* Additional services extract information from the video for search capabilities.

Many enterprises wrap such functionality in a combined web and mobile site to provide streaming video for eLearning.  We are going to look at what it takes to produce the mobile side of such a site in this section.

## The Video Search Application with Media Services

We are going to produce a media services mobile application for this section, based on our last example for Azure Search.  In the new example, this is the approximate flow of the application:

![][img1]

The administrator will upload an MP4 video via the Visual Studio Storage Explorer.  This will be automatically picked up by an Azure Function that encode the video, placing the encoded video into a download area.  The next Azure Function will pick up that video and use Cognitive Services on it to extract information that can be searched and insert that information into the Azure Search instance.  Finally, a third Azure Function will insert the data about the video into a database so that it can be picked up by Azure Mobile Apps.  We are going to use three distinct operations here because encoding and cognitive services are asynchronous - we want to kick them off and let them complete in their own time.

On the client side, we will use the Azure Search instance to find apps, display the information held within Azure Mobile Apps, and allow the user to stream the video using the video player.

As you can see, there are many more services in use in this example than our previous examples:

* [Azure Media Services] is used for video encoding and streaming endpoints.
* [Azure Logic Apps] are used for workflow automation.
* [Azure Functions] are used for automation.
* [Cognitive Services] are used to extract information from the videos.
* [Azure App Service] is used to act as a coordinator for the mobile app.
* [Azure Search] is used as our full text search engine.
* [Azure Storage] is used to store the individual video assets and for some queuing capabilities.
* [SQL Azure] is used as the backing store for the Azure Mobile Apps data store.

This is now a fairly typical mobile application.  We are using 8 different Azure services in a composite manner to provide the facilities needed by our application.

## Creating the Mobile Encoding flow

When I look at the architecture for our mobile backend, I see two distinct parts.  The first is the backend flow that processes the incoming videos.  As videos are uploaded, they need to be injected into a queue.  From there, a series of processes are kicked off to process the incoming video.  First, the video is encoded; then data is extracted from the video for search purposes; finally, the video is added to the SQL database so it can be searched.

The other flow is from the mobile client - it connects to the App Service and makes requests based on what it needs to do.  In this case, we have a set of data tables for providing data about the video and a few custom APIs for handling search and video streaming.

Let's take each of these in turn.   The configuration of most of the services  have already been discussed, so I will not go over them and only provide the options I used.  This includes Storage, Search, SQL Azure, and Functions.

### Creating pre-requisite services

Before I start with the new services, I need an [Azure Storage] account, an [Azure Search] instance and an [Azure Function App].  I've covered all these items in previous sections, so I won't go into them here. The configuration is as follows:

* My [Azure Storage] account called `zumomediach7.core.windows.net` as **General Purpose** storage with **LRS** replication.
* My [Azure Search] instance called `zumomediach7.search.windows.net` in the **Free** pricing tier.
* My [Azure Functions] app called `zumomediach7-functions.azurewebsites.net` in the **Consumption Plan**.  I'm
   using my `zumomediach7` storage account.
* My [SQL Azure] service is called `zumomediach7.database.windows.net`.
* My [SQL Azure] database is called `videosearch` in the **B Basic** pricing plan.
* My [Azure App Service] is created via the **Mobile App** template and called `zumomediach7.azurewebsites.net`.  It
    has an **B1 Basic** app service plan associated with it.

In addition, I've linked the SQL Azure database and storage accounts to the App Service via the Data Connections menu option.  I've also added a query key to my Azure Search instance and provided a pair of App Settings in the App Service - `SEARCH_APIKEY` holds the query key and `SEARCH_ENDPOINT` holds the URI of my service.

Our resource group looks quite extensive now:

![][img2]

Configuration for the services is as follows:

**Azure Storage** has a container for incoming videos called `incoming`.

**Azure Search** has an index with the following fields:

| Field Name | Type | Attributes |
| --- | --- | --- |
| videoId | Edm.String | Key, Retrievable |
| audio | Edm.String | Retrievable, Filterable, Searchable |

The `audio` field is new and will be populated with the textual content from analysis of the video.

**Azure App Service** has a basic TableController which is based on the following DTO model:

```csharp
using Microsoft.Azure.Mobile.Server;

namespace Backend.DataObjects
{
    public class Video : EntityData
    {
        public string Filename { get; set; }

        public string VideoUri { get; set; }
    }
}
```

I also have a custom API that returns my search settings.  I'll add any settings I need to transmit to my mobile app into this same controller so that they can easily be retrieved from the mobile app:

```csharp
using System;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Backend.Controllers
{
    [MobileAppController]
    public class SettingsController : ApiController
    {
        private Controllers.Settings _pSettings;

        public SettingsController()
        {
            _pSettings = new Controllers.Settings
            {
                SearchApiKey = Environment.GetEnvironmentVariable("SEARCH_APIKEY"),
                SearchEndpoint = Environment.GetEnvironmentVariable("SEARCH_ENDPOINT")
            };
        }

        // GET api/Settings
        public Controllers.Settings Get()
        {
            return _pSettings;
        }
    }

    public class Settings
    {
        public string SearchEndpoint { get; set; }
        public string SearchApiKey { get; set; }
    }
}
```

This encompasses information from the majority of the book thus far.  If you are uncertain on how to perform any of this configuration, review the appropriate sections of the book:

* [Chapter 1](../chapter1/firstapp_pc.md) covers creating a Mobile App.
* [Chapter 4](../chapter4/functions.md) covers Azure Functions.
* [This chapter](./search.md) covers Azure Search.

### Creating an Azure Media Services account

So far, we've done a lot of infrastructure work.  We've generated an Azure Mobile App that our mobile app can use to retrieve information about the videos, generated an Azure Search instance with a suitable index, and a storage account for processing the videos.  We now want to move onto the meat of this section - working with video.  In order to do that, we will need an Azure Media Services account.

Creating an Azure Media Services account is very similar to other Azure resources.  Log in to the [Azure Portal] and open the resource group you are using to hold all the resources for this application.

* Click **+ ADD** to add a resource to the resource group.
* Search for **Media Services**, select it, then click on **Create**.
* Fill in the form:
    1. Select a name for your service.  It needs to be unique within the service.
    2. Select your existing storage account (note the limitations on the replication policy if you use your own).
    3. Ensure the region matches your storage account and other resources.
* Click **Create**.

![][img3]

The Media Services accounts may take a couple of minutes to create.  Do not continue until the deployment is complete.

!!! tip "Testing tools for Media Services"
    If you intend to do any development in Azure Media Services, you should download and become familiar with the [Azure Media Services Explorer][5]. this is a test tool for Windows that allows you to upload, download, process, encode and package assets ith Azure Media Services.  You should also grow a collection of test videos.  A great starting point are [these videos from TechSlides][4].

You could stop here and do all the work manually.  If you wish to check out the full set of tutorials, follow the official documentation:

* [Uploading Assets](https://docs.microsoft.com/en-us/azure/media-services/media-services-portal-upload-files)
* [Encoding Assets](https://docs.microsoft.com/en-us/azure/media-services/media-services-portal-encode)
* [Publish Assets](https://docs.microsoft.com/en-us/azure/media-services/media-services-portal-publish)

### The Encoding Pipeline

There is an [excellent sample][2] that uses Azure Functions and Azure Logic Apps as a media workflow.  The Azure Functions do the actual processing, using the Azure Media Services SDK to communicate with the Media Services resource, and the Logic App (below) is used to control the workflow and ensure it works properly.

![Logic App Flow][img4]

To create this flow:

1. Create the Azure Functions required by the flow.  
    *  The source code is in [the referenced project][2]. 
    *  When creating each function,use the **GenericWebhook-CSharp** template.
    *  After creating each function, use the **App Service Editor** (with the Functions settings) to edit the files.  This allows you to create the shared folder and any other files necessary.
    *  Ensure the Functions compile before going further.  
2. 

!!! warn "To Be Continued"
    This section is not complete as yet.  Please check back soon!


<!-- Images -->
[img1]: ./img/media-plan.PNG
[img2]: ./img/media-rg-view.PNG
[img3]: ./img/media-create.PNG
[img4]: ./img/logic-app-flow.png

<!-- Azure Service Definition Overviews -->
[Azure Media Services]: https://docs.microsoft.com/en-us/azure/media-services/media-services-concepts
[Azure Logic Apps]: https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-what-are-logic-apps
[Azure Functions]: https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview
[Cognitive Services]: https://azure.microsoft.com/en-us/services/cognitive-services/
[Azure App Service]: https://docs.microsoft.com/en-us/azure/app-service/app-service-value-prop-what-is
[Azure Search]: https://docs.microsoft.com/en-us/azure/search/search-what-is-azure-search
[Azure Storage]: https://docs.microsoft.com/en-us/azure/storage/storage-introduction
[SQL Azure]: https://docs.microsoft.com/en-us/azure/sql-database/sql-database-technical-overview
[Azure Function App]: https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview
[Samples GitHub repository]: https://github.com/Azure-Samples/media-services-dotnet-functions-integration
[1]: https://github.com/Azure-Samples/media-services-dotnet-functions-integration
[2]: https://github.com/Azure-Samples/media-services-dotnet-functions-integration/tree/master/media-functions-for-logic-app
[3]: https://docs.microsoft.com/en-us/azure/media-services/media-services-custom-mes-presets-with-dotnet
[4]: http://techslides.com/sample-webm-ogg-and-mp4-video-files-for-html5
[5]: http://aka.ms/amse
