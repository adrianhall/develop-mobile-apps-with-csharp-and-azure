using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class TaskListViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializer for the TaskListViewModel
        /// </summary>
        public TaskListViewModel()
        {
            Title = "Task List";

            // Commands
            RefreshCommand = new Command(async () => await Refresh());
            AddNewItemCommand = new Command(async () => await AddNewItem());
            LoadMoreCommand = new Command<TodoItem>(async (TodoItem item) => await LoadMore(item));

            MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
            {
                await Refresh();
            });

            LoadMoreCommand.Execute(null);
        }

        /// <summary>
        /// Set to false when no more items can be loaded
        /// </summary>
        private bool HasMoreItems { get; set; } = true;

        /// <summary>
        /// Reference to the Cloud Service
        /// </summary>
        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();

        #region Bindable Properties
        private ObservableRangeCollection<TodoItem> items = new ObservableRangeCollection<TodoItem>();
        public ObservableRangeCollection<TodoItem> Items
        {
            get { return items; }
            set { SetProperty(ref items, value, "Items"); }
        }

        private TodoItem selectedItem;
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
        #endregion

        #region Commands
        /// <summary>
        /// Bindable property for the AddNewItem Command
        /// </summary>
        public Command AddNewItemCommand { get; }

        /// <summary>
        /// Bindable property for the LoadMore Command
        /// </summary>
        public Command LoadMoreCommand { get; }

        /// <summary>
        /// Bindable property for the Refresh Command
        /// </summary>
        public Command<TodoItem> RefreshCommand { get; }

        /// <summary>
        /// User clicked on the + New Item command
        /// </summary>
        private async Task AddNewItem()
        {
            if (IsBusy)
            {
                return;
            }
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

        /// <summary>
        /// User scrolled beyond the end of the list
        /// </summary>
        /// <param name="item">The item that was activated</param>
        private async Task LoadMore(TodoItem item)
        {
            if (IsBusy || !HasMoreItems)
            {
                return;
            }
            if (Items.Count() > 0 && !Items.Last().Id.Equals(item?.Id))
            {
                return;
            }
            IsBusy = true;
            try
            {
                var list = CloudService.ReadTasksAsync(Items.Count, 20);
                if (list.Count > 0)
                {
                    Items.AddRange(list);
                }
                else
                {
                    HasMoreItems = false;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error Loading Items", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// User clicked on the refresh button (or did a pull-to-refresh command)
        /// </summary>
        private async Task Refresh()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;

            try
            {
                await CloudService.SyncOfflineCacheAsync();
                var identity = await CloudService.GetIdentityAsync();
                if (identity != null)
                {
                    var name = identity.UserClaims.FirstOrDefault(claim => claim.Type.Equals("name")).Value;
                    Title = $"Tasks for {name}";
                }
                var list = CloudService.ReadTasksAsync(Items.Count, 20);
                Items.ReplaceRange(list);
                HasMoreItems = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error Refreshing List", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion
    }
}
