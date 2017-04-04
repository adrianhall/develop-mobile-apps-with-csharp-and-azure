using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Crashes;
using TaskList.Abstractions;
using TaskList.Services;
using Xamarin.Forms;

namespace TaskList
{
    public class App : Application
    {
        public static ICloudService CloudService { get; set; }

        public App()
        {
            MobileCenter.Start("android={your-android-app-secret};ios={your-ios-app-secret}",
                typeof(Crashes));

#if USE_MOCK_SERVICES
            CloudService = new MockCloudService();
#else
            CloudService = new AzureCloudService();
#endif
            MainPage = new NavigationPage(new Pages.EntryPage());
        }
    }
}
