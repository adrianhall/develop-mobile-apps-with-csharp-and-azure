# Your first app - PC Edition

There is a lot of detail to absorb about the possible services that the mobile client can consume and I will go into significant depth on those subjects. First, wouldn't it be nice to write some code and get something working?  Microsoft Azure has a great [first-steps tutorial](https://learn.microsoft.com/azure/developer/mobile-apps/azure-mobile-apps/quickstarts/maui/?pivots=vs2022-windows) that takes you via the quickest possible route from creating a mobile backend to having a functional backend.  I would like to take things a little slower so that we can understand what is going on while we are doing the process.  We will have practically the same application at the end.  The primary reason for going through this slowly is to ensure that all our build and run processes are set up properly.  If this is the first mobile app you have ever written, you will see that there are quite a few things that need to be set up.  This chapter covers the set up required for a Windows PC.  If you wish to develop your applications on a Mac, then skip to the [next section](./mac-intro.md).

The application I am going to build is a simple task list.  The mobile client will have three screens - an entry screen, a task list and a task details page.  I have mocked these pages up using a screen mocking service.

!!! tip
    Mocking your screens before you start coding is a great habit to get into. There are some great tools available including free tools and ideas.  Doing mockups before you start coding is a good way to prevent wasted time later on.  For more tools, see the [tools](../tools.md) section of this book.

![Application Mockups for the Task List][img1]

!!! tip
    If you are using iOS, then you may want to remove the back button as the style guides suggest you don't need one.  Other platforms will need it though, so it's best to start with the least common denominator.  It's the same reason I add a refresh button even though it's only valid on Windows Phone!

My ideas for this app include:

* Tapping on a task title in the task list will bring up the details page.
* Toggling the completed link in the task list will set the completed flag.
* Tapping the spinner will initiate a network refresh.

Now that we have our client screens planned out, we can move onto the thinking about the mobile backend.

## The mobile backend

The mobile backend is an ASP.NET core web API that is served from within Azure App Service: a highly scalable and redundant web hosting service that supports all the major languages.  Azure Mobile Apps is an SDK that creates a mobile-ready web API in ASP.NET Core.  To create the mobile backend, we will use Visual Studio 2022 to create a new Web API project, then add Azure Mobile Apps to it.

First, create a new solution.  You can easily do this using the `dotnet` tool on the command line:

```powershell
PS> mkdir -p \projects\Chapter1
PS> cd \projects\Chapter1
PS> dotnet new sln
PS> .\Chapter1.sln
```

This sequence of commands will create a new empty solution called `Chapter1` and then open it within Visual Studio.  You may be prompted to confirm which version of Visual Studio you wish to open.  Make sure you open Visual Studio 2022.

!!! tip
    Windows 10 and above have an in-built command line tool called [**Terminal**](https://learn.microsoft.com/windows/terminal/) that you can use to easily run command line tools.  Pin it to your task bar for quicker access.

You can also create an empty solution:

* Start Visual Studio 2022.
* In the **Get started** section of the project selector, select **Create a new project**.
* Use the search box to search for **Blank Solution**.
* Enter `Chapter1` as the solution name, then press **Create**.

!!! tip
    Android apps have a hard time with long filenames.  Create a location at the top of the filesystem (for example: `C:\projects`) to hold your mobile app projects so that you are less likely to run into problems later on.

At this point, you'll be at the same point as using the command line tools.

<!-- Images -->
[img1]: assets/mockingbot.png