using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
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
            ICloudService cloudService = ServiceLocator.Instance.Resolve<ICloudService>();
            Table = cloudService.GetTable<TodoItem>();

            Title = "Task List";
            items.CollectionChanged += this.OnCollectionChanged;

            RefreshCommand = new Command(async () => await ExecuteRefreshCommand());
            AddNewItemCommand = new Command(async () => await ExecuteAddNewItemCommand());

            RefreshCommand.Execute(null);

        }

        public ICloudTable<TodoItem> Table { get; set; }
        public Command RefreshCommand { get; }
        public Command AddNewItemCommand { get; }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("[TaskList] OnCollectionChanged: Items have changed");
        }

        ObservableCollection<TodoItem> items = new ObservableCollection<TodoItem>();
        public ObservableCollection<TodoItem> Items
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

        async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var list = await Table.ReadAllItemsAsync();
                Items.Clear();
                foreach (var item in list)
                    Items.Add(item);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TaskList] Error loading items: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteAddNewItemCommand()
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
                Debug.WriteLine($"[TaskList] Error in AddNewItem: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task RefreshList()
        {
            await ExecuteRefreshCommand();
            MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) =>
            {
                await ExecuteRefreshCommand();
            });
        }
    }
}

