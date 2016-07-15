/* Project: TaskList, File: Abstractions/ITable.cs */

using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskList.Abstractions
{
    /// <summary>
    /// Definition of the Table Operations
    /// </summary>
    /// <typeparam name="T">The model type/typeparam>
    public interface ITable<T> where T : class
    {
        /// <summary>
        /// CRUD Operation: CREATE
        /// </summary>
        /// <param name="item">The item to Add</param>
        /// <returns>The inserted item</returns>
        Task<T> CreateAsync(T item);

        /// <summary>
        /// CRUD Operation: READ
        /// </summary>
        /// <param name="id">The id of the item</param>
        /// <returns>The item (or null)</returns>
        Task<T> ReadAsync(string id);

        /// <summary>
        /// CRUD Operation: UPDATE
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <returns>The updated item</returns>
        Task<T> UpdateAsync(T item);

        /// <summary>
        /// CRUD Operations: DELETE
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <returns>The deleted item</returns>
        Task<bool> DeleteAsync(T item);

        /// <summary>
        /// CRUD Operation: READ (BULK)
        /// </summary>
        /// <returns>The list of items</returns>
        Task<List<T>> ListAsync();

        /// <summary>
        /// Synchronize changes with the backend
        /// </summary>
        Task SyncAsync();
    }
}
