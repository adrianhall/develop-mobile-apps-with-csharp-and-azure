/* Project: TaskList, File: Abstractions/IMobileBackend.cs */

using System.Threading.Tasks;

namespace TaskList.Abstractions
{
    /// <summary>
    /// Definition of the connection to the remote backend
    /// </summary>
    public interface IMobileBackend
    {
        /// <summary>
        /// Obtain a reference to a table
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <returns>The ITable definition</returns>
        ITable<T> GetTable<T>() where T : class;

        /// <summary>
        /// Produce the UI and login to the backend system
        /// </summary>
        Task LoginAsync();

        /// <summary>
        /// Produce any UI and log out of the backend system
        /// </summary>
        Task LogoutAsync();
    }
}
