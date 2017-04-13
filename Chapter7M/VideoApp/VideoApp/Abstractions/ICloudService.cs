using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using VideoApp.Models;

namespace VideoApp.Abstractions
{
    public interface ICloudService
    {
        ICloudTable<T> GetTable<T>() where T : TableData;

        Task<MobileServiceUser> LoginAsync();

        Task LogoutAsync();

        Task<AppServiceIdentity> GetIdentityAsync();
    }
}
