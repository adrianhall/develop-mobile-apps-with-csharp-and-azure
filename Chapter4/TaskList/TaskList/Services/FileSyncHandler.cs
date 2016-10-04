using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using System.Threading.Tasks;
using TaskList.Abstractions;
using TaskList.Models;
using Xamarin.Forms;

namespace TaskList.Services
{
    public class FileSyncHandler : IFileSyncHandler
    {
        private IFileSyncProvider fileProvider;
        private AzureCloudService cloudService;

        public FileSyncHandler(AzureCloudService cloudService)
        {
            fileProvider = DependencyService.Get<IFileSyncProvider>();
            this.cloudService = cloudService;
        }

        public Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
            => fileProvider.GetDataSourceAsync(metadata);

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete)
            {
                await fileProvider.DeleteLocalFileAsync(file);
            }
            else
            {
                if (file.TableName.ToLowerInvariant().Equals("todoitem"))
                {
                    await fileProvider.DownloadFileAsync<TodoItem>(file, cloudService.TaskTable);
                }
            }
        }
    }
}
