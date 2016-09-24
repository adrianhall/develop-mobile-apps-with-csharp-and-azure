using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Models;

namespace TaskList.Abstractions
{
    public interface ICloudService
    {
        Task<MobileServiceUser> LoginAsync();
        Task LogoutAsync();
        Task<AppServiceIdentity> GetIdentityAsync();
        Task SyncOfflineCacheAsync();

        // Custom APIs
        Task<StorageTokenViewModel> GetSasTokenAsync();

        // The TodoItem table
        Task<TodoItem> UpsertTaskAsync(TodoItem item);
        Task DeleteTaskAsync(TodoItem item);
        Task<ICollection<TodoItem>> ReadTasksAsync(int? start, int? count);
        Task<TodoItem> ReadTaskAsync(string id);
    }
}
