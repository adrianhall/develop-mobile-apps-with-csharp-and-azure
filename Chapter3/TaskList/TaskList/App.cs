using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Services;
using Xamarin.Forms;

namespace TaskList
{
    public class App : Application
    {
        public App()
        {
            ServiceLocator.Add<ICloudService, AzureCloudService>();
            MainPage = new NavigationPage(new Pages.EntryPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
