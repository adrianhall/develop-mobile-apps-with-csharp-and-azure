# Introduction

It's been over 20 years since the first iPhone was released, ushering in a new era of development.  In that time,
iOS and Android development has become a requirement for almost any client-side developers as everyone has embraced
doing more on their phones.  At the same time, the landscape of development options has grown significantly.  Previously,
mobile app development required you to know very low-level languages like C++ or Objective-C.  Even the higher-level
languages required some specialist knowledge to get the best out of the platorm.  Nowadays, developers have a plethora 
of language and framework options:

![The languages and media for mobile development](media/introduction/frameworks.png)

Joining the stable of traditional languages like C#, JavaScript, and Java are new languages like Dart, Swift, and 
Kotlin. There is also an increasing demand for cross-platform tools like Progressive Web Apps (PWAs), .NET MAUI and 
Flutter.  Modern app development also highly leverages the "cloud" - a set of services hosted by service providers
like Amazon, Google, and Microsoft to enable you to write better apps in a shorter amount of time to meet your 
customers demands.

With this book, you will use [C#] and [Azure] to:

* Understand how to use cross-platform development tools to design and build modern mobile apps.
* Understand how Platform-as-a-Service offerings from Azure can speed up your development time.
* Learn how to build cloud-connected applications for iOS and Android.
* Understand what's involved in running a production backend service for your mobile applications.
* Learn how to leverage DevOps techniques to run a production mobile backend service.

To best leverage this book, you should be an intermediate to experienced C# developer who has already built a mobile app
and wants to take their app to the next level by integrating the cloud.  

This book is not for a beginner developer.  I expect you to know how to develop mobile apps in C#.  If you are just starting
out in development, first learn the basics of the language - [C#].  Then move to building mobile apps that are not cloud
connected.  The [.NET MAUI] site has a selection of tutorials.  Although you do not need to understand web application 
development to get value out of this book, you should at least build a Web API with [ASP.NET] and understand how to send
web API calls to your backend.  The first chapter provides a walk-through of setting up your development environment and
building your first mobile application if you are new to this area.  However, you should spend lots of time understanding
the technology as you move forward.

## What are cloud-connected mobile apps?

When I refer to a **mobile application** or **mobile app**, I mean every piece of software that is related to the application you
want to use.  This include, for example, the **mobile client**.  The mobile client is the piece of code you run on your iPhone 
or Android phone.  It also includes the **mobile backend**, which is the service that you run in the cloud to provide important
services to your mobile client.

A **cloud-connected mobile app** is a mobile client that connects to a mobile backend for shared services.  Most of the apps
on your phone are cloud-connected already.  For example, Instagram uses the cloud for authentication and photo storage.  Even
the humble todo app is normally cloud-connected.  The tasks are stored in the cloud so that you can retrieve them from
multiple devices (such as a web client or your desktop PC).

## Why is cross-platform native development important?

It should come as no surprise to anyone - Apple and Google have won the mobile OS wars.  All smart phones being sold today run
either iOS or Android.  However, these are two very different mobile operating systems with different programming models.  iOS
is usually programmed in Swift, whereas Android is programmed in Kotlin.  If you want to develop for both platforms (and you 
should to reach the maximum market), then you need to know both Switch and Kotlin, plus the vagaries of both platforms.  That's a 
tall order even for the most dedicated mobile developer.

There are alternatives for cross-platform development.  The biggest entries into this area are:

* [React Native], coded in [JavaScript].
* [Flutter], coded in [Dart].
* [.NET MAUI], which replaces [Xamarin.Forms], coded in [C#].

All three support the iOS and Android platforms.  Each of these also has other potential targets, including Windows and MacOS.  This
allows you to build apps that can target mobile and desktop with the same logic.  For business application developers, this can
be a huge bonus.  Almost 80% of the average app is "business logic" with the rest of the app being dedicated to the UI.  Even if
you were to write unique UI code for each platform, the bulk of the code is the same.

## What features use the cloud?

A cloud-connected mobile app will use one or more of the following highly-desired services:

* Authentication.
* Storage of structured data (like a task list).
* Storage of unstructured files (like photographs).
* Push notifications.
* Telemetry.

In addition, most mobile apps will pull from a set of additional services.  For example, consumer apps normally leverage an advertising
service for monetization, whereas a shopping app will need a payment service for processing credit card transactions.  You may also
need to leverage machine learning or custom compute services.  The cloud is a good choice for all of these and you will find multiple
options for each one.

------

## About the author

For over 15 years, Adrian Hall has been developing enterprise mobile applications and SDKs to aid the developer in
moving operations to the cloud. He is the current maintainer of Azure Mobile Apps (a .NET SDK for developing offline
first mobile, desktop, and web applications) and regularly speaks at conferences on mobile development.

------

## How to use this book

This book is designed to be read while you are in front of a mobile development system, coding along with the tutorials
as you go.  As such, it is a good idea to work your way from beginning to end.  However, each chapter stands alone, so 
you can jump to an individual chapter if you want to touch on a single topic.

To get started, press the button below.

[Set up your development system :octicon-rocket-24:](chapter1/)
