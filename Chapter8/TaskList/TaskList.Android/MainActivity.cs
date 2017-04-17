using Android.App;
using Android.Content.PM;
using Android.OS;

namespace TaskList.Droid
{
	[Activity (
		Label = "TaskList",
		Icon = "@drawable/icon",
		Theme="@style/MainTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
			LoadApplication (new TaskList.App ());
		}
	}
}

