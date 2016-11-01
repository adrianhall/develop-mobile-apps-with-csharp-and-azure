using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using TaskList.Models;

namespace TaskList.Abstractions
{
    public interface ICloudService
    {
        ICloudTable<T> GetTable<T>() where T : TableData;

        Task<MobileServiceUser> LoginAsync();

        Task LogoutAsync();

        Task<AppServiceIdentity> GetIdentityAsync();
    }
}
