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
            ServiceLocator.Add<ICloudService, AzureCloudService>();
            MainPage = new NavigationPage(new Pages.VideoList());
        }
    }
}
