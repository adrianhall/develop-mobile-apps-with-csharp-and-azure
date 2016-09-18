using System;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class TaskDetailViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializer for the view model
        /// </summary>
        /// <param name="item">The task that this view model is for</param>
        public TaskDetailViewModel(TodoItem item = null)
        {
            CurrentTask = item ?? new TodoItem { Text = "New Item", Complete = false };
            Title = CurrentTask.Text;

            SaveCommand = new Command(async () => await SaveAsync());
            DeleteCommand = new Command(async () => await DeleteAsync());
        }

        /// <summary>
        /// Reference to the Cloud Service
        /// </summary>
        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();

        #region Bindable Properties
        private TodoItem currentTask;
        public TodoItem CurrentTask
        {
            get { return currentTask; }
            set { SetProperty(ref currentTask, value, "CurrentTask"); }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Bindable Property for the Save Command
        /// </summary>
        public Command SaveCommand { get; }

        private async Task SaveAsync()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;

            try
            {
                await CloudService.UpsertTaskAsync(CurrentTask);
                MessagingCenter.Send<TaskDetailViewModel>(this, "ItemsChanged");
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Save Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Bindable property for the Delete command
        /// </summary>
        public Command DeleteCommand { get; }

        private async Task DeleteAsync()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;

            try
            {
                if (CurrentTask.Id != null)
                {
                    await CloudService.DeleteTaskAsync(CurrentTask);
                    MessagingCenter.Send<TaskDetailViewModel>(this, "ItemsChanged");
                }
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Delete Failed", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion
    }
}
