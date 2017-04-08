When developing mobile applications, certain cloud services - data access, authentication and push notifications - almost always make their way into the requirements for the mobile backend.  However, there are a number of other Azure services that can also make an appearance.

Azure has a number of "platform as a service" type services.  We've built Azure Mobile Apps on top of some of them - App Service and Notification Hubs.  PaaS, as it is known, is a microservice where you don't have to deal with the underlying operating system or scaling issues.  While we won't cover all possible Azure services, it's worth mentioning a few of the more useful ones here.  In addition, there are some things you can't get from Azure.  We'll take a look at those later on.

We will be covering [Search][1], [Realtime Communications][2] and [Video][3] services within this chapter in more depth.

## Azure Services

The following services are available with your Azure subscription.  The all incur some kind of cost, although search has a free tier.

### Cognitive Services

Machine Learning has been a hot topic in development circles recently.  It's highly complex and very specific to the application being handled.  Fortunately, some applications are relatively easily generalized.  For example, if I want to speak to my app (for example, like Siri or Cortana), then I can use [Cognitive Services][11] to process the speech.  There are SDKs for speech processing, textual language analytics and image processing.  The image processing SDKs are particularly useful for mobile photo applications.

### Content Delivery Networks and Traffic Manager

I hope your application is successful.  If you enjoy worldwide success or your mobile backend just can't go down, you will want to augment the single region solution with multiple region availability.  Perhaps you just want an automated failover to another region within the country, or perhaps you want to support other continents with closer capabilities.  Whatever the reason, you will want to take a look at [Traffic Manager][5], which routes a HTTP request to the closest service endpoint.

In addition, you may want to make static assets available closer to the user.  You can't do anything about dynamic content.  However, static assets like images, videos or long-lived documents can be published to a [Content Delivery Network][6] or CDN.  The Azure CDN uses multiple providers (Akamai and Verizon) so that you can choose the footprint and costs that you need.

### DocumentDB

We have concentrated on integrating SQL services with offline-sync capabilities in this book.  However, there is a whole different paradigm for storing data on the server, mostly known as NoSQL stores.  These use JSON blobs to store their data.  Their main advantage is that you don't have to think about the schema of the data.    The Azure entry into this market is called [DocumentDB][9].  It's highly scalable, geo-replicated, and allows you to define business logic in JavaScript to be run on the server (as stored procedures and triggers).

NoSQL databases tend to be prevalent in gaming, social data and IoT applications.  SQL tends to be more prevalent in web and enterprise applications.  Both types of data store have their place in the development world.  It really depends on what sort of work load you are considering with your app.  For assistance with the choice, check out the [NoSQL vs. SQL][10] article.

### Media services

One of the major pieces of functionality that I see in a lot of mobile applications is video.  It may be an advertisement, a training video, or a major video platform like Netflix or Hulu.  Video is a complex subject and can take up [a book][7] just by itself.  Fortunately, [Azure Media Services][8] simplifies the process by providing a scalable platform for video encoding and streaming to your mobile clients.  It also integrates media analytics, the ability to use cognitive services, and content protection in the platform.  We are going to go [more in-depth][3] in Media Services later on.

### Search

If you have some sort of catalog (for example, a shopping cart or video library), then it's likely you will want to search within the catalog or library.  This is where [Azure Search][5] comes in.  It is a platform service that implements an easily consumed API for searching your catalog.  This goes beyond basic SQL queries and gets into natural language processing, fuzzy search, proximity search, term boosting and regular expressions.  It supports 56 languages and provides feedback to your user in terms of search suggestions, highlighting and faceted navigation.  We are going to do a more [in-depth study][1] of Azure Search later in the chapter.

### Service Fabric

Not all mobile applications are straight-forward view-based applications.  Some are games, for example, or highly scalable interactive platforms.  When you need custom microservices on the backend, then [Azure Service Fabric][4] will provide an architectural framework for developing the most complex, low-latency, data-intensive scenarios.  In these cases, a simple CRUD model is never going to be enough architecturally for the job of powering your backend.  However, such complexity comes at a cost.  In this case, the cost is the developer time necessry to invest in a highly scalable custom backend for your data flows.  If you think that you need more than basic CRUD style APIs because of latency concerns, then Service Fabric is the appropriate service.

## Non-Azure Services

Please note that I do not receive compensation and do not endorse any of the companies or products listed in this section.  Please do your research and ensure that the product will assist you.

### Customer Feedback

You can get some information from your customers without asking.  Crash analytics and page-use analytics can be driven by code within your application and submitted in real-time or batched to an application analytics package like [Mobile Center][25] or [App Insights][26].  You may, however, need to ask your customers to leave feedback.  It could be as simple as a button click (like the Visual Studio happy face / sad face), or it may be more extensive, providing support capabilities, star ratings, and free-form text.  You can use these insights to drive UX improvements or features of your mobile app.

Some companies that provide mobile-based feedback systems include [OpinionLab][27] and [Apptentive][28].

### In App Purchases and Mobile Advertising

One of the big topics that mobile application developers face is how to monetize (i.e. get paid) their app.  Mobile app buyers do not like paying a lot for an app, so supplementary income comes from in-app purchases and mobile advertising.  In-app purchases (or In-app billing, as Android folks call it) are driven via an SDK provided by the mobile platform - [iOS][16] or [Android][17].  You will need to integrate these SDKs into the platform-specific project, and Xamarin has some tools to assist for [iOS][18] and [Android][19].

Mobile Advertising is another good area for investigation.  There are a number of considerations when choosing a mobile advertising partner (which is the partner who will feed your app advertisements to display and pay you for their display).  The good news for most mobile app developers is that you can use the social authentication provider as a hint as to which mobile ad provider is going to be appropriate:

* Facebook uses [Facebook App Ads][21]
* Google uses [AdMob][20]

There are also other mobile ad providers, like [OGMobi][24], [Smaato][22] and [AdIquity][23], should you not be using one of these social auth providers.  It is not uncommon to see top-20 lists of mobile ad networks, so do some research - in particular, pay attention to the "fill rate" for customers in your area and the performance of the network (which is generally referred to as eCPM or effective cost per mille - one thousand impressions).  Your mobile ad performance will be less if the advertisements being offered by the network are not appropriate for your customers.

In general, you will need to integrate the ad network SDK into your application and add view elements to your pages where the mobile ads will be displayed.

### Real-time Communications

You might want to get an instant alert of a pending change to a table, or perhaps produce the next great chat app.  Real-time Communications keeps a TCP or UDP channel open to the mobile client for communications, allowing you to send instant alerts to the connected mobile clients.  This enables quite a few collaborative use cases. There
are quite a few "standards" here:

* [WebRTC][12]
* [WebSockets][13]
* [SignalR][14]
* [socket.io][15]

WebRTC uses UDP underneath, so it's better for lossy communication with low latency - for example, audio or video.  WebSockets (and the frameworks that depend on it, like socket.io and SignalR) use TCP.  This means higher latency, but lossless communication.  Since we are developing an ASP.NET mobile backend and using C# for the mobile application, SignalR would be more appropriate to use. You can find [a full example of integrating SignalR with Xamarin Forms on GitHub][29].

<!-- Links -->
[1]: ./search.md
[2]: ./realtime.md
[3]: ./media.md
[4]: https://azure.microsoft.com/en-us/services/service-fabric/
[5]: https://azure.microsoft.com/en-us/services/traffic-manager/
[6]: https://azure.microsoft.com/en-us/services/cdn/
[7]: https://www.amazon.com/Technology-Video-Audio-Streaming/dp/0240805801/ref
[8]: https://azure.microsoft.com/en-us/services/media-services/
[9]: https://azure.microsoft.com/en-us/services/documentdb/
[10]: https://docs.microsoft.com/en-us/azure/documentdb/documentdb-nosql-vs-sql
[11]: https://azure.microsoft.com/en-us/services/cognitive-services/
[12]: https://webrtc.org/
[13]: https://www.html5rocks.com/en/tutorials/websockets/basics/
[14]: http://signalr.net/
[15]: http://socket.io/
[16]: https://www.raywenderlich.com/122144/in-app-purchase-tutorial
[17]: https://developer.android.com/google/play/billing/billing_integrate.html
[18]: https://developer.xamarin.com/guides/ios/application_fundamentals/in-app_purchasing/
[19]: https://components.xamarin.com/gettingstarted/xamarin.inappbilling
[20]: https://www.google.com/admob/
[21]: https://developers.facebook.com/products/ads/
[22]: https://www.smaato.com/
[23]: http://adiquity.com/app-developers-overview/
[24]: https://ogmobi.com/
[25]: https://mobile.azure.com/signup?utm_medium=referral_link&utm_source=GitHub&utm_campaign=ZUMO%20Book
[26]: https://azure.microsoft.com/en-us/services/application-insights/
[27]: http://www.opinionlab.com/tour/give-your-customer-a-voice/mobile-feedback/
[28]: https://www.apptentive.com/
[29]: https://github.com/schneidenbach/Xamarin-Forms-and-SignalR-Example
