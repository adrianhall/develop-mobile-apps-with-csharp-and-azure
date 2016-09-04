# Introduction

Welcome to my first book.  It's free, it's open source, and it's comprehensive.
Those attributes also describe two of my favorite technologies, and I'm basing
my first book on them.  The first is [Xamarin Forms][1] , a technology that
allows you to develop cross-platform mobile applications using C# and the .NET
framework.  The second is [Azure Mobile Apps][2], a technology that allows
you to connect your mobile app to  resources that are important in cloud
connected mobile applications such as table data, authentication, and push
notifications.

This book does not tell you everything there is to know about either topic.  It
focuses on the topics necessary to get your mobile apps connected to the cloud.

## What are Cloud Connected Mobile Apps?

I guess I should define some of the terminology that I am going to use.  When I
refer to a **mobile application** or **mobile app**, I mean every piece of
software that is related to the application you want to use.  This includes, for
example, the **mobile client**. This is the piece of code you run on your iPhone
or Android phone.  It also includes the **mobile backend** which is the service
that you run in the cloud to provide important services to your mobile client.

A **cloud connected mobile application** is a mobile client that connects to a
mobile backend for shared services.  Quite a few of the apps on your phone are
cloud connected already.  For example, Instagram uses the cloud for photo
storage, and Facebook uses the cloud to store the news feeds of you and your
friends.

## Why Cross-Platform Native Development is important?

It should come as no surprise that Apple and Google have pretty much won the
mobile OS wars.  Over 90% of the smart phones being sold today run either iOS
or Android.  However, these are two very different mobile operating systems,
with different programming models.  iOS is based on either [Swift][21] or
Objective-C.  Android is based on [Java][22].  If you want to develop for the
80% case (and you should), then you need to know both Swift and Java.  That's
a tall order even for the most dedicated mobile developer.

However, there are alternatives out there.  Most notable, you can write your
mobile application with one code-base and just write exceptions for when the
platforms diverge.  You have to pick a single language and a tool set that
supports cross-platform development to do this.  Not all cross-platform tool
sets are created equal, however.  Some do not compile your code to native
binaries, which means that you do not get access to all the functionality of
the mobile platforms you are targeting.

Xamarin, recently acquired by Microsoft, allows you to target all major
platforms - iOS, Android and Windows - to gain greater than 95% coverage of
the mobile smart phone market.  It does this by leveraging the .NET framework
and compiling your code to a native binary for each platform.

Xamarin.Forms is a cross-platform framework, based on XAML and .NET, that
allows you to use common UI pages to develop your apps.

## Why Azure Mobile Apps?

When you think about the major apps in the marketplace for each mobile
platform, the thing that they have in common is that they have some sort
of cloud infrastructure driving them.  It might be as simple as storing
your task list, or as complex as your Facebook news feed.  It could be a
gaming leader board, or the social sharing of your photos.  Whatever it
is, cloud connectivity is a must.

Not all clouds are created equal.  There are some common features that you
should think about including irrespective of the application.  I like to use
Azure Mobile Apps for these features because they are all included and you
can get started with most of the features for zero cost.  Even the features
that cannot be obtained without spending a little money are relatively cheap.

!!! info
    **Azure Mobile Apps** is a feature of Azure App Service.  Azure App Service
    is a collection of services that commonly are used together to develop modern
    Internet Apps.  This includes web hosting, API hosting and Mobile SDKs.

### Features of Cloud Connected Mobile Apps

A cloud connected mobile application will use one or more services in the
following areas:

* Authentication
* Storage of structured data (like a task list)
* Storage of unstructured data (like photographs)
* Push notifications
* Invocation of Custom Code

I am going to cover each of these in great detail.  In addition, I will also
cover some common issues and solutions that developers run into while developing
cloud connected mobile applications such as testing and going to production.

Aside from the actual features of mobile apps, there are other things to
consider while developing your mobile application.  Here is my list, in no
particular order:

1. Continuous Deployment
2. Slots or Staging Sites
3. Automatic Scalability
4. Database Backups
5. Combined Web

The point here is that my intent is to write a production quality application.
I need to be able to deploy my site with confidence without resorting to jumping
through hoops.  I want to run multiple versions of the backend so that I can run
a staging site for testing purposes.  I want to be able to roll back my
production site to a previous version at a moments notice.  I want to be able to
handle the load when my app is successful, and I want things to be backed up
(since bad things inevitably happen when I am least prepared).

All of these features are available in Azure App Service, and the Mobile Apps
SDK that I will use throughout the book is supported only on Azure App Service.

## Who is This Book For?

This book is for intermediate to experienced C# developers who have already
built a mobile app with Xamarin and want to take their mobile apps to the next
level by utilizing cloud services.

This book is not for the beginner.  Explicitly, I already expect you to know how
to develop mobile applications with C# and Xamarin technologies.  If you are
unfamiliar with the C# language, you can get started with a free course on the
Internet.  The basics of the language can be learned at [www.learncs.org][3].
Once you have the language basics under your belt, you can move on to building
mobile applications with Xamarin. You can learn more about developing cross-platform
mobile development with Xamarin at the [Xamarin][5] website.  Although you do
not need to understand ASP.NET to get value out of this book, be aware that the
mobile back ends that I will be covering are written in C# and ASP.NET.  A good
understanding of ASP.NET will assist you.

### Things You Should Know!

Before you get started with development, spend some time learning the tools of
the trade.  The command prompt on the Mac is [bash][12] and the command prompt
on the PC is [PowerShell][13].  You should be proficient in the shell on the
platforms that you use.

Additionally, you should become familiar with the source code control system
that you will use.  For most, this means becoming familiar with
[git][14].  Don't even think of developing without using source control.

## What You Will Need

The list of hardware and software for mobile development is longer than your
typical development projects.  It is still, thankfully, relatively short and
easy to acquire.

### Hardware

You will want a computer on which to develop code.  If you develop iOS
applications, then you **MUST** have a Mac running the latest version of Mac
OSX.  If you develop Universal Windows applications, then you **MUST** have a
PC running Windows 10.  Android applications can be developed on either platform.

My own experience has taught me that the tooling for developing mobile backends
in C# and ASP.NET (our primary languages during the course of this book) are
better on a PC running Windows 10.  Thus, my hardware choice is a rather beefy
[Windows 10 PC][7] for my main development system.  In addition, I have a
[Mac Mini][6] underneath my desk that I use to build the iOS portions of the
applications.

### Software

All of the following software are freely available.  You should install each
package and update it (if appropriate) so that it is fully patched.

#### On your Mac

* [XCode][8] (available on the Mac App Store)
* [Xamarin Studio][9]
* [Android Studio and Tools][23] (if you intend to build Android apps on the Mac)

You must run XCode at least once after installation so that you can accept the
license agreement.

#### On your Windows PC

* [Android Studio and Tools][23]
* [Visual Studio Community][10]
* [Azure SDK][11]

When installing Visual Studio, you will want to install the components for
Web applications and Cross-platform Mobile development.  If you have already
installed Visual Studio and did not install these components, run the installer
again to add the components.

!!! tip
    Development Tools are big, multi-gigabyte installers.  If you are on a slow or
    restricted link, you may want to download the installers onto a thumb drive for
    local installation.

### Cloud Services

You will need an Azure account to complete most of the tutorials in this book.
In fact, you won't be able to get very far without one. If you have an MSDN
account, you already have access to free Azure resources.  You just need to log
into your [MSDN account][15] and activate your Azure benefit.  Students may be
able to get access to [Dreamspark][16] from school resources, but this is not
suitable for developing mobile applications.  This is because storage costs
money.  If you don't have MSDN, then there is a [free trial][17] available.
Once the trial period ends, you can move to a Pay-As-You-Go account and continue
to use free services without incurring a charge. I'll point out when you are
going to incur charges on your Azure account, but I will be using free resources
most of the time.

Aside from Azure resources, you will want some place to store your code.  This
doesn't have to be in the cloud.  If you want to use the cloud, you can use
GitHub or Visual Studio Team Services.  Both are free to use.  GitHub provides
public repositories for free.  Visual Studio Team Services provides private
respositories for free.  Visual Studio Team Services also includes other
services that I will talk about during the course of the book, some of which may
incur cost.  I will be publishing all my samples and tutorial code on GitHub so
that you can easily download it.  You don't have to use one of these resources,
but I won't be covering other service usage.

You will need a **Developer Account** for the appropriate app store if you
intend to distribute your mobile clients or if you intend to use specific cloud
services.  Apple is specific - if you intend to use push notifications or
distribute iOS apps, then you need an [Apple Developer Account][18],
[Google Developer Account][19] and/or [Windows Store Developer Account][20].
The terms of the accounts are changed constantly, so review the current terms
when you sign up.  My recommendation is to defer signing up for these programs
until you need something they offer.

Now, let's get developing!  Our next section is dependent on where you are developing:

* On a Mac, skip ahead to the [Mac section][int-mac].
* On a PC, the [next section][int-pc] covers Visual Studio.

[int-mac]: ./chapter1/firstapp_mac.md
[int-pc]: ./chapter1/firstapp_pc.md

[1]: https://www.xamarin.com/forms
[2]: https://aka.ms/azuremobileapps
[3]: http://www.learncs.org/
[4]: http://www.asp.net/
[5]: https://developer.xamarin.com/
[6]: http://www.apple.com/shop/buy-mac/mac-mini?product=MGEQ2LL/A&step=config#
[7]: http://www.intel.com/content/www/us/en/nuc/change-the-game-with-nuc.html
[8]: https://itunes.apple.com/us/app/xcode/id497799835?mt=12
[9]: https://www.xamarin.com/platform
[10]: https://www.visualstudio.com/products/visual-studio-community-vs
[11]: https://azure.microsoft.com/en-us/downloads/
[12]: http://guide.bash.academy/
[13]: https://mva.microsoft.com/en-us/training-courses/getting-started-with-powershell-3-0-jump-start-8276
[14]: https://try.github.io/levels/1/challenges/1
[15]: https://msdn.microsoft.com/en-us/default.aspx
[16]: https://www.dreamspark.com/Product/Product.aspx?productid=99
[17]: https://azure.microsoft.com/en-us/free/
[18]: https://developer.apple.com/programs/
[19]: https://play.google.com/apps/publish/signup
[20]: https://developer.microsoft.com/en-us/store/register
[21]: https://developer.apple.com/swift/
[22]: https://developer.android.com/training/index.html
[23]: https://developer.android.com/
