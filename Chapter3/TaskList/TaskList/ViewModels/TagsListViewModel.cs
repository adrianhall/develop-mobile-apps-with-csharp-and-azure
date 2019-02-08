using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class TagsListViewModel : BaseViewModel
    {
        bool hasMoreItems = true;

        public TagsListViewModel()
        {
            Title = "Tags";

            RefreshCommand = new Command(async () => await Refresh());
            AddNewItemCommand = new Command(async () => await AddNewItem());
            LogoutCommand = new Command(async () => await Logout());
            TasksCommand = new Command(async () => await NavigateToTasks());
            LoadMoreCommand = new Command<Tag>(async (Tag item) => await LoadMore(item));

            // Subscribe to events from the Task Detail Page
            MessagingCenter.Subscribe<TagDetailViewModel>(this, "ItemsChanged", async (sender) =>
            {
                await Refresh();
            });

            // Execute the refresh command
            RefreshCommand.Execute(null);
        }

        private async Task NavigateToTasks()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new Pages.TaskList());
        }

        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();
        public IPlatform PlatformProvider => DependencyService.Get<IPlatform>();
        public ICommand RefreshCommand { get; }
        public ICommand AddNewItemCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand TasksCommand { get; }

        ObservableRangeCollection<Tag> items = new ObservableRangeCollection<Tag>();
        public ObservableRangeCollection<Tag> Items
        {
            get { return items; }
            set { SetProperty(ref items, value, "Items"); }
        }

        Tag selectedItem;
        public Tag SelectedItem
        {
            get { return selectedItem; }
            set
            {
                SetProperty(ref selectedItem, value, "SelectedItem");
                if (selectedItem != null)
                {
                    Application.Current.MainPage.Navigation.PushAsync(new Pages.TagDetail(selectedItem));
                    SelectedItem = null;
                }
            }
        }

        async Task Refresh()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                await CloudService.SyncOfflineCacheAsync();
                var table = await CloudService.GetTableAsync<Tag>();
                var list = await table.ReadItemsAsync(0, 20);
                Items.ReplaceRange(list);
                hasMoreItems = true; // Reset for refresh
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Items Not Loaded", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task AddNewItem()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Pages.TagDetail());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Item Not Added", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task Logout()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                await CloudService.LogoutAsync();
                Application.Current.MainPage = new NavigationPage(new Pages.EntryPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Logout Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task LoadMore(Tag item)
        {
            if (IsBusy)
            {
                Debug.WriteLine($"LoadMore: bailing because IsBusy = true");
                return;
            }

            // If we are not displaying the last one in the list, then return.
            if (!Items.Last().Id.Equals(item.Id))
            {
                Debug.WriteLine($"LoadMore: bailing because this id is not the last id in the list");
                return;
            }

            // If we don't have more items, return
            if (!hasMoreItems)
            {
                Debug.WriteLine($"LoadMore: bailing because we don't have any more items");
                return;
            }

            IsBusy = true;
            var table = await CloudService.GetTableAsync<Tag>();
            try
            {
                var list = await table.ReadItemsAsync(Items.Count, 20);
                if (list.Count > 0)
                {
                    Debug.WriteLine($"LoadMore: got {list.Count} more items");
                    Items.AddRange(list);
                }
                else
                {
                    Debug.WriteLine($"LoadMore: no more items: setting hasMoreItems= false");
                    hasMoreItems = false;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("LoadMore Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

