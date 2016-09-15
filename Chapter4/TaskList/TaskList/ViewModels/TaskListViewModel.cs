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
        /// Set to false when no more items can be loaded
        /// </summary>
        private bool HasMoreItems { get; set; } = true;

        public TaskListViewModel()
        {
            Title = "Task List";

            // Commands
            RefreshCommand = new Command(async () => await Refresh());
            AddNewItemCommand = new Command(async () => await AddNewItem());
            LogoutCommand = new Command(async () => await Logout());
            LoadMoreCommand = new Command<TodoItem>(async (TodoItem item) => await LoadMore(item));

            MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
            {
                await Refresh();
            });

            RefreshCommand.Execute(null);
        }

        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();

        public Command AddNewItemCommand { get; }
        public Command LoadMoreCommand { get; }
        public Command LogoutCommand { get; }
        public Command<TodoItem> RefreshCommand { get; }

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
    }
}
