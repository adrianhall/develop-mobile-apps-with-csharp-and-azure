using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using PCLStorage;
using PCLStorage.Exceptions;
using System;
using System.Threading.Tasks;
using TaskList.Abstractions;

[assembly: Xamarin.Forms.Dependency(typeof(TaskList.Droid.Services.DroidFileSyncProvider))]
namespace TaskList.Droid.Services
{
    public class DroidFileSyncProvider : IFileSyncProvider
    {
        /// <summary>
        /// Delete a file from the local file source
        /// </summary>
        /// <param name="file">The file to delete</param>
        public async Task DeleteLocalFileAsync(MobileServiceFile file)
        {
            var itemFolder = await GetLocalStorageAsync(file.TableName, file.ParentId);
            try
            {
                var item = await itemFolder.GetFileAsync(file.Name);
                await item.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
                // Ignore this error
            }
        }

        /// <summary>
        /// Download the specified file to local storage.
        /// </summary>
        /// <typeparam name="T">The type of the associated sync table</typeparam>
        /// <param name="file">The MobileServiceFile for the file to download</param>
        /// <param name="table">The associated local sync table reference</param>
        public async Task DownloadFileAsync<T>(MobileServiceFile file, IMobileServiceSyncTable<T> table)
        {
            var itemFolder = await GetLocalStorageAsync(file.TableName, file.ParentId);
            await table.DownloadFileAsync(file, PortablePath.Combine(itemFolder.Path, file.Name));
        }

        /// <summary>
        /// Obtains the data source for a local file request
        /// </summary>
        /// <param name="metadata">The metadata for the local file request</param>
        /// <returns>An IMobileServiceFileDataSource</returns>
        public async Task<IMobileServiceFileDataSource> GetDataSourceAsync(MobileServiceFileMetadata metadata)
        {
            var itemFolder = await GetLocalStorageAsync(metadata.ParentDataItemType, metadata.ParentDataItemId);
            return new PathMobileServiceFileDataSource(PortablePath.Combine(itemFolder.Path, metadata.FileName));
        }

        /// <summary>
        /// Given a MobileServiceFile, locate the proper file path on the disk
        /// </summary>
        /// <param name="file">The MobileServiceFile</param>
        /// <returns>The path to the MobileServiceFile</returns>
        private async Task<IFolder> GetLocalStorageAsync(string tablename, string id)
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var baseFolder = await FileSystem.Current.LocalStorage.GetFolderAsync(basePath);

            // Find the folder for the table, creating it if it does not exist
            var tableFolder = await baseFolder.CreateFolderAsync(tablename, CreationCollisionOption.OpenIfExists);

            // Find the folder for the item, creating it if it does not exist, and return it
            return await tableFolder.CreateFolderAsync(id, CreationCollisionOption.OpenIfExists);
        }
    }
}