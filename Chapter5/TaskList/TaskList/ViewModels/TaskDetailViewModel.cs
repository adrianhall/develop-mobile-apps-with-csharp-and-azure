using System;
using System.Diagnostics;
using System.Linq;
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
            SaveCommand = new Command(async () => await SaveAsync());
            DeleteCommand = new Command(async () => await DeleteAsync());
            if (item != null)
            {
                CurrentTask = item;
                Title = item.Text;
            }
            else
            {
                CurrentTask = new TodoItem { Text = "New Item", Complete = false };
                Title = "New Item";
            }
        }

        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();
        public IPlatform PlatformProvider => DependencyService.Get<IPlatform>();
        public Command SaveCommand { get; }
        public Command DeleteCommand { get; }

        TodoItem currentTask;
        public TodoItem CurrentTask
        {
            get { return currentTask; }
            set { SetProperty(ref currentTask, value, "CurrentTask"); }
        }


        public string Text
        {
            get
            {
                return CurrentTask.Text;
            }
            set
            {
                var cText = CurrentTask.Text;
                SetProperty(ref cText, value, "Text");
                CurrentTask.Text = cText;
            }
        }

        public bool Complete
        {
            get
            {
                return CurrentTask.Complete;
            }
            set
            {
                var cComplete = CurrentTask.Complete;
                SetProperty(ref cComplete, value, "Complete");
                CurrentTask.Complete = cComplete;
            }
        }

        async Task SaveAsync()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var table = await CloudService.GetTableAsync<TodoItem>();
                await table.UpsertItemAsync(CurrentTask);
                await CloudService.SyncOfflineCacheAsync();
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

        async Task DeleteAsync()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                if (CurrentTask.Id != null)
                {
                    var table = await CloudService.GetTableAsync<TodoItem>();
                    await table.DeleteItemAsync(CurrentTask);
                    await CloudService.SyncOfflineCacheAsync();
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
