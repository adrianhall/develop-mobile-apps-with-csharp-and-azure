using System;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Helpers;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.ViewModels
{
    public class TagDetailViewModel : BaseViewModel
    {
        public TagDetailViewModel(Tag item = null)
        {
            SaveCommand = new Command(async () => await Save());
            DeleteCommand = new Command(async () => await Delete());

            if (item != null)
            {
                Item = item;
                Title = item.TagName;
            }
            else
            {
                Item = new Tag { TagName = "Tag" };
                Title = "New Tag";
            }

        }

        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();
        public IPlatform PlatformProvider => DependencyService.Get<IPlatform>();
        public Command SaveCommand { get; }
        public Command DeleteCommand { get; }

        public Tag Item { get; set; }

        async Task Save()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var table = await CloudService.GetTableAsync<Tag>();
                await table.UpsertItemAsync(Item);
                await CloudService.SyncOfflineCacheAsync();
                MessagingCenter.Send<TagDetailViewModel>(this, "ItemsChanged");
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
                    var table = await CloudService.GetTableAsync<Tag>();
                    await table.DeleteItemAsync(Item);
                    await CloudService.SyncOfflineCacheAsync();
                    MessagingCenter.Send<TagDetailViewModel>(this, "ItemsChanged");
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

