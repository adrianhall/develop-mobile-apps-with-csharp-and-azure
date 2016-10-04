using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Threading.Tasks;

namespace TaskList.Abstractions
{
    public interface IFileSyncProvider
    {
        Task DeleteLocalFileAsync(MobileServiceFile file);
        Task DownloadFileAsync<T>(MobileServiceFile file, IMobileServiceSyncTable<T> table);
        Task<IMobileServiceFileDataSource> GetDataSourceAsync(MobileServiceFileMetadata metadata);
    }
}
