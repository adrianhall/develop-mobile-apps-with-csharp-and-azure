using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Services;
using Xamarin.Forms;

namespace TaskList
{
    public class App : Application
    {
        public App(string loadParameter = null)
        {
            ServiceLocator.Instance.Add<ICloudService, AzureCloudService>();

            if (loadParameter == null)
            {
                MainPage = new NavigationPage(new Pages.EntryPage());
            }
            else
            {
                MainPage = new NavigationPage(new Pages.PictureView(loadParameter));
            }
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
