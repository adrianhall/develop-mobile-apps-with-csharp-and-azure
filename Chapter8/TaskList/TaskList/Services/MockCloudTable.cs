using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskList.Abstractions;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace TaskList.Services
{
    public class MockCloudTable<T> : ICloudTable<T> where T : TableData
    {
        private readonly Dictionary<string, T> items = new Dictionary<string, T>();
        private int currentVersion = 1;

        public async Task<T> CreateItemAsync(T item)
        {
            item.Id = Guid.NewGuid().ToString("N");
            item.CreatedAt = DateTimeOffset.Now;
            item.UpdatedAt = DateTimeOffset.Now;
            item.Version = ToVersionString(currentVersion++);
            items.Add(item.Id, item);
            return item;
        }

        public async Task DeleteItemAsync(T item)
        {
            if (item.Id == null)
            {
                throw new NullReferenceException();
            }
            if (items.ContainsKey(item.Id))
            {
                items.Remove(item.Id);
            }
            else
            {
                throw new MobileServiceInvalidOperationException("Not Found", null, null);
            }
        }

        public async Task<ICollection<T>> ReadAllItemsAsync()
        {
            List<T> allItems = new List<T>(items.Values);
            return allItems;
        }

        public async Task<T> ReadItemAsync(string id)
        {
            if (items.ContainsKey(id))
            {
                return items[id];
            }
            else
            {
                throw new MobileServiceInvalidOperationException("Not Found", null, null);
            }
        }

        public async Task<T> UpdateItemAsync(T item)
        {
            if (item.Id == null)
            {
                throw new NullReferenceException();
            }
            if (items.ContainsKey(item.Id))
            {
                item.UpdatedAt = DateTimeOffset.Now;
                item.Version = ToVersionString(currentVersion++);
                items[item.Id] = item;
                return item;
            }
            else
            {
                throw new MobileServiceInvalidOperationException("Not Found", null, null);
            }
        }

        private byte[] ToVersionString(int i)
        {
            byte[] b = BitConverter.GetBytes(i);
            string str = Convert.ToBase64String(b);
            return Encoding.ASCII.GetBytes(str);
        }
    }
}
