using Android.App;
using Android.Content.PM;
using Android.OS;
using VideoApp.Abstractions;
using VideoApp.Droid.Services;
using Xamarin.Forms;

namespace VideoApp.Droid
{
	[Activity (Label = "VideoApp", Icon = "@drawable/icon", Theme="@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			global::Xamarin.Forms.Forms.Init (this, savedInstanceState);

			var loginProvider = (DroidLoginProvider)DependencyService.Get<ILoginProvider>();
			loginProvider.Init(this);

			LoadApplication (new VideoApp.App());
		}
	}
}

