using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class TaskListViewModel : BaseViewModel
    {

        public TaskListViewModel()
        {
            CloudTable = CloudService.GetTable<TodoItem>();

            Title = "Task List";

            RefreshCommand = new Command(async () => await Refresh());
            AddNewItemCommand = new Command(async () => await AddNewItem());
            LogoutCommand = new Command(async () => await Logout());
            LoadMoreCommand = new Command<TodoItem> (async (TodoItem item) => await LoadMore(item));

            // Subscribe to events from the Task Detail Page
            MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
            {
                await Refresh();
            });

            // Execute the refresh command
            RefreshCommand.Execute(null);
        }

        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();
        public IPlatform PlatformProvider => DependencyService.Get<IPlatform>();
        public ICloudTable<TodoItem> CloudTable { get; set; }
        public ICommand RefreshCommand { get; }
        public ICommand AddNewItemCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand LoadMoreCommand { get; }

        ObservableRangeCollection<TodoItem> items = new ObservableRangeCollection<TodoItem>();
        public ObservableRangeCollection<TodoItem> Items
        {
            get { return items; }
            set { SetProperty(ref items, value, "Items"); }
        }

        TodoItem selectedItem;
        public TodoItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                SetProperty(ref selectedItem, value, "SelectedItem");
                if (selectedItem != null)
                {
                    Application.Current.MainPage.Navigation.PushAsync(new Pages.TaskDetail(selectedItem));
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
                var identity = await CloudService.GetIdentityAsync();
                if (identity != null)
                {
                    var name = identity.UserClaims.FirstOrDefault(c => c.Type.Equals("name")).Value;
                    Title = $"Tasks for {name}";
                }
                var list = await CloudTable.ReadItemsAsync(0, 20);
                Items.ReplaceRange(list);
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
                await Application.Current.MainPage.Navigation.PushAsync(new Pages.TaskDetail());
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

        async Task LoadMore(TodoItem item)
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var list = await CloudTable.ReadItemsAsync(Items.Count, 20);
                if (list.Count > 0)
                {
                    Items.AddRange(list);
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
