#pragma warning disable RCS1090 // Call 'ConfigureAwait(false)'.

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

#pragma warning disable RCS1174 // Remove redundant async/await.
            LoginCommand = new Command(async () => await ExecuteLoginCommand());
#pragma warning restore RCS1174 // Remove redundant async/await.
        }

        public Command LoginCommand { get; }
        public string AppService { get; set; }

        private async Task ExecuteLoginCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var cloudService = ServiceLocator.Instance.Resolve<ICloudService>();
                await cloudService.LoginAsync();
                await cloudService.RegisterForPushNotifications();
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
