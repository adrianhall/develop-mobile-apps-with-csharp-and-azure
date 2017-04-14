using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VideoApp.Abstractions;
using VideoApp.Helpers;
using VideoApp.Models;
using Xamarin.Forms;

namespace VideoApp.ViewModels
{
    public class VideoListViewModel : BaseViewModel
    {
        ICloudService cloudService;

        public VideoListViewModel()
        {
            cloudService = ServiceLocator.Resolve<ICloudService>();
            Table = cloudService.GetTable<Video>();
            Title = "Videos";

            RefreshCommand = new Command(async () => await ExecuteRefreshCommand());
            RefreshCommand.Execute(null);
        }

        public ICloudTable<Video> Table { get; set; }
        public Command RefreshCommand { get; }


        ObservableRangeCollection<Video> items = new ObservableRangeCollection<Video>();
        public ObservableRangeCollection<Video> Items
        {
            get { return items; }
            set { SetProperty(ref items, value, "Items"); }
        }

        Video selectedItem;
        public Video SelectedItem
        {
            get { return selectedItem; }
            set
            {
                SetProperty(ref selectedItem, value, "SelectedItem");
                Application.Current.MainPage.Navigation.PushAsync(new Pages.VideoDetail(selectedItem));
            }
        }

        async Task ExecuteRefreshCommand()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var list = await Table.ReadAllItemsAsync();
                Items.ReplaceRange(list);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Cannot load video list", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
