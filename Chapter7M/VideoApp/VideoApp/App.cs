using Plugin.MediaManager.Forms;
using VideoApp.Abstractions;
using VideoApp.Helpers;
using VideoApp.Services;
using Xamarin.Forms;

namespace VideoApp
{
   public class App : Application
    {
        public App()
        {
            // Make sure the linker doesn't remove the plugin
            var workaround = typeof(VideoView);

            ServiceLocator.Add<ICloudService, AzureCloudService>();
            MainPage = new NavigationPage(new Pages.VideoList());
        }
    }
}
