---
title: Setting up your development environment
---

When I sit down to code, I am generally sitting in one place for a few hours at a time.  Such is the nature of development. We are hyper-focused on achieving a result.  Time flies by.  Nothing breaks the mood than eye strain, hand cramps, or a sore back.  For this reason, the first thing you should do is find somewhere you are comfortable coding. It should be well ventilated and have the proper lighting.  You should have a good desk and chair so you are comfortable typing for a long period of time.  Consider how the sunlight moves when positioning your desk.  You don't need a big desk, but you do need one that is the right height and can keep everything you need in easy reach.

## The hardware

Next, let's talk about the computer you need.  If you are going to build mobile apps, you will want a Mac.  Apple intentionally goes out of their way to ensure that you stay within their ecosystem when you are building, testing, and distributing iOS apps.  There is simply no way around it.  In order to distribute apps on the iOS app store, you need to use XCode 12 or later.  If you are looking to do mobile app development, make sure your Mac has:

* MacOSX Monterey or later.
* 8GB of RAM
* 512GB SSD

This will limit the models you can use.

* Macbook (2016 or later)
* Macbook Pro (2015 or later)
* Macbook Air (2015 or later)
* Mac Mini (2014 or later)
* iMac (2015 or later)
* iMac Pro (all current models)
* Mac Pro (2013 or later)
* Mac Studio (all current models)

It doesn't matter (at the moment) whether you choose an ARM64 processor (like the M1 or M2) or an Intel processor.  However, I expect that Apple will deal a blow to the Intel processor support in the near future.  If you are looking for a model that will last a few years, choose one with an ARM64 processor.  I use a Mac mini with 8GB of RAM and a 512GB SSD for all my work.

Your work doesn't stop with the base unit.  You will also want a monitor and a comfortable mouse and keyboard.  Yes, you can code on the retina display of the Macbook Pro, but size does matter.  I find the real estate on a large dedicated monitor to be preferable, especially when I am trying to see the specific pixel alignment of a UI element.  For the keyboard and mouse, I find the Apple keyboards too cramped for my hands, so an external keyboard is preferable.  I use the Logitech K860 ergonomic keyboard and a Logitech MX Master 3S mouse, but you should find a keyboard that is comfortable for you.  Remember, you are going to be working at the computer for hours at a stretch.  You need to be comfortable.

In terms of hardware, you are almost done.  You need to have a target device.  A lot of development can be done against the simulators (iOS) or emulators (Android) that are provided as development tools.  However, nothing beats the actual devices when it comes to testing for your users.  The performance is different between the simulated device and a real device.  You will bump into issues because something you thought worked doesn't work on a real device.  I use my own phone for the iOS work, but I also keep a lookout for older phones on auction sites so I can use them for testing apps.  Your test phones don't need cellular service - most of them work with just Wifi.

Talking of which... yes - you need an Internet link.  You use it for research, for working with the Azure cloud, and for downloading all the libraries you need to build your app.  It doesn't need to be fast.  I used a 1.5Mbps DSL link for a long time.  A reliable Internet connection is required for any sort of mobile app development.

## The services

There aren't a lot of services you need at this point.

* [Apple Developer Program]
* [Azure Cloud]
* A source code repository provider

As I mentioned earlier, Apple intentionally makes it impossible to develop mobile apps for iOS without committing to their ecosystem.  You must have an Apple Developer Program membership to distribute iOS apps.  However, you will find that you need the membership during development as well.  While it is possible to build and test apps without the program membership, you will find certain things just don't work.  As an example, features like authentication require keychain access.  Keychain access requires custom entitlements, and custom entitlements requires an Apple Developer Program membership when you use the simulator.  Authentication is one of the key cloud capabilities that you are going to use.  The Apple Developer Program costs $99 per year (US pricing).

The Azure Cloud is the other service you will need.  This is where you will run your mobile backends.  The Azure Cloud does not cost anything when you are not using it.  In addition, several services are free when used in "developer" mode.  Things only start costing money when you use them in production.  There are also several mechanisms by which you can get a free account - through your school, as a startup, or through a Visual Studio subscription from your business.  If you don't qualify for a free account, you can sign up for free and Microsoft will give you some credits to get you started.

It's worthwhile getting to know your way around the Azure Cloud.  In particular, you should start by understanding [how to put cost controls and alerts in place](https://docs.microsoft.com/azure/cost-management-billing/costs/cost-mgt-alerts-monitor-usage-spending) so you don't have a surprise bill.  One of the biggest complaints about the cloud occurs when you don't understand the pricing of the services that you are choosing.  You end up running a service that is too large for your needs, or you forget to shut the service down when it is no longer required.  Both of these can result in continual billing that is not discovered until it comes time to pay for it.  Set up alerts so that you are warning before you hit the limits.

Finally, you will want somewhere to store the projects you write.  I use [GitHub](https://github.com/) for the contents of the book and for all my little projects.  Most of the projects are not public.  GitHub allows you to choose between public and private repositories.  You can also use [GitLab](https://gitlab.com/), [BitBucket](https://bitbucket.org) or [Azure DevOps Services](https://azure.microsoft.com/services/devops/) if you want an enterprise centric full-lifecycle management solution.

## The software

The final step of configuring the development environment is setting up the software.  Let's start with the mobile development tools:

* [XCode]
* [Android Studio]

XCode is distributed on the Mac App Store, and Android Studio is distributed by Google.  You should take some time to properly configure them:

* Open XCode and accept all the licenses.
* [Install any additional simulators](https://developer.apple.com/documentation/xcode/installing-additional-simulator-runtimes) you want to use.
* Add your Apple Developer Program and GitHub accounts to the settings.

Surprisingly, Apple doesn't give you a lot of help in associating XCode with your online accounts.  The account linkage process starts from **Preferences** > **Accounts**.  The sign in process is straight forward.

Remember that you need to be running XCode 12 or later to distribute applications.  Aside from distributing apps, you will also use the XCode command-line tools and simulators and you will manage provisioning profiles for your apps through XCode.

Android Studio comprises multiple pieces - the actual studio app, plus the Android SDK and the Emulator.  You need to ensure you update the Android SDK with the following:

* An API version:
    * The SDK itself
    * The ARM64 image with Google APIs.
    * The ARM64 image with Google Play APIs.

One of the problems with Android development is deciding which API versions to target.  When developing an Android app, you specify a "minimum" and "target" API version.  The target version is the version that the app is compiled against, whereas the "minimum" API version is the minimum version supported (and that affects the APIs that you can use).  Most developers strive to keep the minimum API version as low as possible so that you can reach the maximum audience.  Google requires a target API version of 30 or greater (as of August 2021) in order to be published on the Google Play Store.

Install the target API version.  I would also recommend that you install the minimum API version so that you can run your app on the minimum supported API version.  Once the SDK is installed, use the AVD Manager to [create an emulator](https://developer.android.com/studio/run/managing-avds).

Other common tools include:

* Terminal apps - I use [iTerm2](https://iterm2.com).
* A good shell - I use zsh and the [oh-my-zsh](https://ohmyz.sh) extensions.
* [Homebrew](https://brew.sh) - use it to [install and configure git](https://github.com/git-guides/install-git) for your chosen source code repository service.
* A good programmers font with powerline ligatures - I use [Cascadia Code](https://github.com/microsoft/cascadia-code).
* A text editor - I use [Visual Studio Code](https://code.visualstudio.com).
* A good image editor - I use [Gimp](https://www.gimp.org), although I also use [ImageMagick](https://imagemagick.org), which you can install with Homebrew.

Take some time to learn about the tools you have just installed.  Understanding how to use each of these tools will help you be productive later on.

## .NET Development Tools

To do .NET development, you will need:

* The .NET runtime
* The MAUI workloads
* Visual Studio 2022 for Mac (Preview)
* The Azure CLI


<!-- Links -->
[Apple Developer Program]: https://developer.apple.com/programs/
[Azure Cloud]: https://azure.com/free
[XCode]: https://developer.apple.com/xcode/
[Android Studio]: https://developer.android.com/studio
