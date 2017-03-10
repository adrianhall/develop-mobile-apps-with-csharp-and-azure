using TodoList.Abstractions;
using TodoList.Services;
using Xamarin.Forms;

namespace TodoList
{
    public class App : Application
    {
        public static ICloudService CloudService { get; set; }

        public App()
        {
            CloudService = new AzureCloudService();
            MainPage = new NavigationPage(new Pages.EntryPage());
        }
    }
}
