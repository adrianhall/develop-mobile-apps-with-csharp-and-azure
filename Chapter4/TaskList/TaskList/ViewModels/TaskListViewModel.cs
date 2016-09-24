using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// <summary>
        /// Initializer for the TaskListViewModel
        /// </summary>
        public TaskListViewModel()
        {
            Title = "Task List";

            // Commands
            RefreshCommand = new Command(async () => await RefreshAsync());
            AddNewItemCommand = new Command(async () => await AddNewItemAsync());
            AddNewFileCommand = new Command(async () => await AddNewFileAsync());
            LoadMoreCommand = new Command<TodoItem>(async (TodoItem item) => await LoadMoreAsync(item));

            MessagingCenter.Subscribe<TaskDetailViewModel>(this, "ItemsChanged", async (sender) => await RefreshAsync());

            RefreshCommand.Execute(null);
        }

        /// <summary>
        /// Set to false when no more items can be loaded
        /// </summary>
        private bool HasMoreItems { get; set; } = true;

        /// <summary>
        /// Reference to the Cloud Service
        /// </summary>
        public ICloudService CloudService => ServiceLocator.Get<ICloudService>();

        /// <summary>
        /// Reference to the Platform Provider
        /// </summary>
        public IPlatform PlatformProvider => DependencyService.Get<IPlatform>();

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

        private bool isUploadingFile;
        public bool IsUploadingFile
        {
            get { return isUploadingFile; }
            set { SetProperty(ref isUploadingFile, value, "IsUploadingFile"); }
        }

        private Double fileProgress = 0.0;
        public Double FileProgress
        {
            get { return fileProgress;  }
            set { SetProperty(ref fileProgress, value, "FileProgress"); }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Bindable property for the AddNewItem Command
        /// </summary>
        public ICommand AddNewItemCommand { get; }

        /// <summary>
        /// Bindable property for the LoadMore Command
        /// </summary>
        public ICommand LoadMoreCommand { get; }

        /// <summary>
        /// Bindable property for the Refresh Command
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Bindable property for the AddNewFile Command
        /// </summary>
        public ICommand AddNewFileCommand { get; }

        /// <summary>
        /// User clicked on the + New Item command
        /// </summary>
        private async Task AddNewItemAsync()
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
        private async Task LoadMoreAsync(TodoItem item)
        {
            if (IsBusy || !HasMoreItems)
            {
                return;
            }
            if (Items.Any() && !Items.Last().Id.Equals(item?.Id))
            {
                return;
            }
            IsBusy = true;
            try
            {
                var list = await CloudService.ReadTasksAsync(Items.Count, 20);
                if (list.Any())
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
        private async Task RefreshAsync()
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
                var list = await CloudService.ReadTasksAsync(Items.Count, 20);
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

        /// <summary>
        /// User clicked on the Add New File button
        /// </summary>
        private async Task AddNewFileAsync()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;

            try
            {
                // Get a stream for the file
                var mediaStream = await PlatformProvider.GetUploadFileAsync();
                if (mediaStream == null)
                {
                    IsBusy = false;
                    return;
                }

                // Get the SAS token from the backend
                var storageToken = await CloudService.GetSasTokenAsync();

                // Use the SAS token to get a reference to the blob storage
                var storageUri = new Uri($"{storageToken.Uri}{storageToken.SasToken}");
                var blobStorage = new CloudBlockBlob(storageUri);

                // Get the length of the stream
                var mediaLength = mediaStream.Length;

                // Initialize the blocks
                int bytesInBlock = 1024;                // The number of bytes in a single block
                var buffer = new byte[bytesInBlock];    // The buffer to hold the data during transfer
                int totalBytesRead = 0;                 // The number of bytes read from the stream.
                int bytesRead = 0;                      // The number of bytes read per block.
                int blocksWritten = 0;                  // The # Blocks Written

                IsUploadingFile = true;
                FileProgress = 0.00;

                // Loop through until we have processed the whole file
                do
                {
                    // Read a block from the media stream
                    bytesRead = mediaStream.Read(buffer, 0, bytesInBlock);

                    if (bytesRead > 0)
                    {
                        // Move the buffer into a memory stream
                        using (var memoryStream = new MemoryStream(buffer, 0, bytesRead))
                        {
                            string blockId = GetBlockId(blocksWritten);
                            await blobStorage.PutBlockAsync(blockId, memoryStream, null);
                        }

                        // Update the internal counters
                        totalBytesRead += bytesRead;
                        blocksWritten++;

                        // Update the progress bar
                        FileProgress = totalBytesRead / mediaLength;
                    }

                } while (bytesRead > 0);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error Uploading File", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                IsUploadingFile = false;
                FileProgress = 0.0;
            }
        }

        /// <summary>
        /// Convert the Block ID to the string we need
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private string GetBlockId(int block)
        {
            char[] tempID = new char[6];
            string iStr = block.ToString();

            for (int j = tempID.Length - 1; j > (tempID.Length - iStr.Length - 1); j--)
            {
                tempID[j] = iStr[tempID.Length - j - 1];
            }
            byte[] blockIDBeforeEncoding = Encoding.UTF8.GetBytes(tempID);
            return Convert.ToBase64String(blockIDBeforeEncoding);
        }
        #endregion
    }
}
