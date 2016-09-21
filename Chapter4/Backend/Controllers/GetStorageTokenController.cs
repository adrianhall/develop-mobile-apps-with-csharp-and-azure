using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Backend.Controllers
{
    [MobileAppController]
    public class GetStorageTokenController : ApiController
    {
        private const string connString = "CUSTOMCONNSTR_MS_AzureStorageAccountConnectionString";
        private const string containerName = "userdata";

        /// <summary>
        /// Initialize the /api/GetStorageToken area
        /// </summary>
        public GetStorageTokenController()
        {
            ConnectionString = Environment.GetEnvironmentVariable(connString);
            StorageAccount = CloudStorageAccount.Parse(ConnectionString);
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        /// <summary>
        /// The connection string for the Azure Storage area
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Reference to the Azure Storage SDK
        /// </summary>
        public CloudStorageAccount StorageAccount { get; }

        /// <summary>
        /// Reference to the connected blob storage account
        /// </summary>
        public CloudBlobClient BlobClient { get; }

        [Authorize]
        public async Task<StorageTokenViewModel> GetAsync()
        {
            // The userId is the SID without the sid: prefix
            var claimsPrincipal = User as ClaimsPrincipal;
            var userId = claimsPrincipal
                .FindFirst(ClaimTypes.NameIdentifier)
                .Value.Substring(4);

            // Errors creating the storage container result in a 500 Internal Server Error
            var container = BlobClient.GetContainerReference(containerName);
            try
            {
                await container.CreateIfNotExistsAsync();
            }
            catch (StorageException ex)
            {
                Debug.WriteLine($"Cannot create container: {ex.Message}");
            }

            // Get the user directory within the container
            var directory = container.GetDirectoryReference(userId);
            var blobName = Guid.NewGuid().ToString("N");
            var blob = directory.GetBlockBlobReference(blobName);

            // Create a policy for accessing the defined blob
            var blobPolicy = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(60),
                Permissions = SharedAccessBlobPermissions.Read
                            | SharedAccessBlobPermissions.Write
                            | SharedAccessBlobPermissions.Create
            };

            return new StorageTokenViewModel
            {
                Name = blobName,
                Uri = blob.Uri,
                SasToken = blob.GetSharedAccessSignature(blobPolicy)
            };
        }
    }

    /// <summary>
    /// ViewModel for the Storage Token response - used for serialization
    /// to JSON.
    /// </summary>
    public class StorageTokenViewModel
    {
        public string Name { get; set; }
        public Uri Uri { get; set; }
        public string SasToken { get; set; }
    }
}
