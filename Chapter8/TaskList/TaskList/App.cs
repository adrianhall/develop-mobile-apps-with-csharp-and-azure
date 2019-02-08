using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
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
#if __IOS__ || __ANDROID__
            MobileCenter.Start("android=609b2734-0353-4e71-a654-fedd9df1632a;ios=af1c12ba-85ae-4be8-b5cd-73fe79c9f697",
                typeof(Analytics), typeof(Crashes));
#endif

#if USE_MOCK_SERVICES
            CloudService = new MockCloudService();
#else
            CloudService = new AzureCloudService();
#endif
            MainPage = new NavigationPage(new Pages.EntryPage());
        }
    }
}
