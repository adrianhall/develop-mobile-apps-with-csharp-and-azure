using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class TaskDetailViewModel : BaseViewModel
    {
        public TaskDetailViewModel(TodoItem item = null)
        {
            CloudTable = CloudService.GetTable<TodoItem>();
            SaveCommand = new Command(async () => await Save());
            DeleteCommand = new Command(async () => await Delete());

            if (item != null)
            {
                Item = item;
                Title = item.Text;
            }
            else
            {
                Item = new TodoItem { Text = "New Item", Complete = false };
                Title = "New Item";
            }

        }

        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();
        public IPlatform PlatformProvider => DependencyService.Get<IPlatform>();
        public ICloudTable<TodoItem> CloudTable { get; set; }
        public Command SaveCommand { get; }
        public Command DeleteCommand { get; }

        public TodoItem Item { get; set; }

        async Task Save()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                await CloudTable.UpsertItemAsync(Item);
                MessagingCenter.Send<TaskDetailViewModel>(this, "ItemsChanged");
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Save Item Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task Delete()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                if (Item.Id != null)
                {
                    await CloudTable.DeleteItemAsync(Item);
                    MessagingCenter.Send<TaskDetailViewModel>(this, "ItemsChanged");
                }
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Delete Item Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
