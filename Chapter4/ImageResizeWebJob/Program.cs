using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Configuration;

namespace ImageResizeWebJob
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("[ImageResizeWebJob] Initializing WebJob");
            var connectionString = ConfigurationManager.ConnectionStrings["MS_AzureStorageAccountConnectionString"].ConnectionString;
            Console.WriteLine($"[ImageResizeWebJob] Connection String = {connectionString}");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Ensure the userdata area exists
            Console.WriteLine("[ImageResizeWebJob] Initializing User Data");
            CloudBlobContainer userdata = blobClient.GetContainerReference("userdata");
            userdata.CreateIfNotExists();

            // Ensure the publicdata area exists
            Console.WriteLine("[ImageResizeWebJob] Initializing Public Data");
            CloudBlobContainer publicdata = blobClient.GetContainerReference("publicdata");
            publicdata.CreateIfNotExists();

            // Ensure the delete queue exists
            Console.WriteLine("[ImageResizeWebJob] Initializing Delete Queue");
            CloudQueue queue = queueClient.GetQueueReference("delete");
            queue.CreateIfNotExists();

            // Start running the WebJob
            Console.WriteLine("[ImageResizeWebJob] Starting Job Host");
            var host = new JobHost();
            host.RunAndBlock();
        }
    }
}
