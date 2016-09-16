using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using TaskList.Abstractions;
using TaskList.Droid.Services;
using Xamarin.Forms;

namespace TaskList.Droid
{
    [Activity(Label = "TaskList.Droid", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Initialize Azure Mobile Apps
            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            // Initialize Xamarin Forms
            global::Xamarin.Forms.Forms.Init(this, bundle);

            // Initialize the Platform Provider (must be done after the dependency service is set up)
            ((DroidPlatform)DependencyService.Get<IPlatform>()).Init(this);

            // Load the application
            LoadApplication(new App());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
        }
    }
}

