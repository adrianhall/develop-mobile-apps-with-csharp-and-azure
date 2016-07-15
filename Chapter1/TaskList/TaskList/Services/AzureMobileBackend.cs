using System.Threading.Tasks;
using TaskList.Abstractions;

namespace TaskList.Services
{
    /// <summary>
    /// Concrete implementation of <see cref="IMobileBackend"/> that connects to 
    /// an Azure Mobile Apps backend.
    /// </summary>
    public class AzureMobileBackend : IMobileBackend
    {
        /// <summary>
        /// Creates a new Mobile Backend client connection
        /// </summary>
        public AzureMobileBackend()
        {
            Client = new MobileServiceClient("https://eb3961b4-820c-4be0-ae9e-539120358e72.azurewebsites.net");
        }

        /// <summary>
        /// Reference to the <see cref="MobileServiceClient"/> object we
        /// created in the constructor
        /// </summary>
        public MobileServiceClient Client { get; }

        #region IMobileBackend Implementation
        /// <summary>
        /// Obtain a reference to a table
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <returns>The ITable definition</returns>
        public ITable<T> GetTable<T>()
        {
            return new AzureMobileTable<T>(Client);
        }

        /// <summary>
        /// Produce the UI and login to the backend system
        /// </summary>
        public Task LoginAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Produce any UI and log out of the backend system
        /// </summary>
        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
