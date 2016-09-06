# Android Developer Notes

This chapter contains random notes that I discovered while developing mobile apps
with Xamarin Forms on the Android platform.  I hope they are useful to you.

## Handling Callbacks with an Android SDK

### Dissecting the Google Plus Login Process

## Missing libaot-mscorlib.dll.so

When running an application is debug mode, I sometimes saw the following deployment issue:

```bash
D/Mono    ( 1366): AOT module 'mscorlib.dll.so' not found: dlopen failed: library "/data/app-lib/TaskList.Droid-2/libaot-mscorlib.dll.so" not found
```

To fix this:

* Right-click on the **Droid** project and select **Properties**.
* Select the **Android Options** tab.
* Uncheck the **Use Fast Deployment** option.
* Save the properties sheet.
* Redeploy the application.

## Fixing Errors with the Visual Studio Emulator for Android

One of the issues I found while running on the Visual Studio Emulator for Android involved debugging.  The Android app 
starts, then immediately closes and debugging stops.  In the output window, you see `Could not connect to the debugger`.
To fix this:

* Close the Android Emulator window.
* Open the **Hyper-V Manager**.
* Right-click the emulator you are trying to use and select **Settings...**.
* Expand the **Processor** node and select **Compatibility**.
* Check the **Migrate to a physical computer with a different processor version** box.
* Click on **OK**.

It's a good idea to do this on all the emulators.  When you start the emulator, this error should be gone.


