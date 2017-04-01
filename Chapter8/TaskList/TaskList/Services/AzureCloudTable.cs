using Microsoft.WindowsAzure.MobileServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskList.Abstractions;

namespace TaskList.Services
{
    public class AzureCloudTable<T> : ICloudTable<T> where T : TableData
    {
        private readonly MobileServiceClient client;
        private readonly IMobileServiceTable<T> table;

        public AzureCloudTable(MobileServiceClient client)
        {
            this.client = client;
            this.table = client.GetTable<T>();
        }

        public async Task<T> CreateItemAsync(T item)
        {
            await table.InsertAsync(item);
            return item;
        }

        public async Task DeleteItemAsync(T item)
        {
            await table.DeleteAsync(item);
        }

        public async Task<ICollection<T>> ReadAllItemsAsync()
        {
            return await table.ToListAsync();
        }

        public async Task<T> ReadItemAsync(string id)
        {
            return await table.LookupAsync(id);
        }

        public async Task<T> UpdateItemAsync(T item)
        {
            await table.UpdateAsync(item);
            return item;
        }
    }
}
