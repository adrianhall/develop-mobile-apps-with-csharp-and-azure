using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskList.Services
{
    /// <summary>
    /// Concrete implementation of the <see cref="ITable{T}"/> interface that
    /// uses a connected Azure Mobile Apps Backend to perform online operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class AzureMobileTable<T> : ITable<T> where T : class
    {
        public AzureMobileTable(AzureMobileBackend client)
        {
            Client = client;
            Table = client.GetTable<T>();
        }

        /// <summary>
        /// Storage for the connected Azure Mobile Backend object
        /// </summary>
        AzureMobileBackend Client { get; }

        /// <summary>
        /// Storage for the connected Azure Mobile Apps table reference
        /// </summary>
        public MobileServiceTable Table { get; }

        #region ITable<T> Implementation
        /// <summary>
        /// CRUD Operation: CREATE
        /// </summary>
        /// <param name="item">The item to Add</param>
        /// <returns>The inserted item</returns>
        public async Task<T> CreateAsync(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CRUD Operation: READ
        /// </summary>
        /// <param name="id">The id of the item</param>
        /// <returns>The item (or null)</returns>
        public async Task<T> ReadAsync(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CRUD Operation: UPDATE
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <returns>The updated item</returns>
        public async Task<T> UpdateAsync(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CRUD Operations: DELETE
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <returns>The deleted item</returns>
        public async Task<bool> DeleteAsync(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CRUD Operation: READ (BULK)
        /// </summary>
        /// <returns>The list of items</returns>
        public async Task<List<T>> ListAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Synchronize changes with the backend
        /// </summary>
        public async Task SyncAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
