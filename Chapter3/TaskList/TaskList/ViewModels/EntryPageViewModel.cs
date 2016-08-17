using System;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Helpers;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class EntryPageViewModel : BaseViewModel
    {
        public EntryPageViewModel()
        {
            Title = "Task List";

            AppService = Locations.AppServiceUrl;

            LoginCommand = new Command(async () => await Login());
        }

        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();

        public IPlatform PlatformProvider => DependencyService.Get<IPlatform>();

        public Command LoginCommand { get; }

        public string AppService { get; set; }

        async Task Login()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                await CloudService.LoginAsync();
                Application.Current.MainPage = new NavigationPage(new Pages.TaskList());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Login Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
