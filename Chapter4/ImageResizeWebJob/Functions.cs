using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImageResizeWebJob
{
    public class Functions
    {
        static int requiredHeight = 600;
        static int requiredWidth = 800;

        public static void ImageUploaded(
            [BlobTrigger("userdata/{name}.{ext}")] Stream input,
            string name,
            string ext,
            [Queue("delete")] out string path)
        {
            Console.WriteLine($"[ImageUploaded] Trigger userdata/{name}.{ext}");
            if (!ext.ToLowerInvariant().Equals("png"))
            {
                Console.WriteLine($"[ImageUploaded] userdata/{name}.{ext} - not a PNG file (queue for deletion)");
                path = $"userdata/{name}.{ext}";
                return;
            }

            // Read the blob stream into an Image object
            var image = Image.FromStream(input);

            // Process the image object
            Console.WriteLine($"[ImageUploaded] size = {image.Width} x {image.Height}");
            if (image.Height > requiredHeight || image.Width < requiredWidth)
            {
                Console.WriteLine("[ImageUploaded] Need to resize the image");
                var destRect = new Rectangle(0, 0, requiredWidth, requiredHeight);
                var destImage = new Bitmap(requiredWidth, requiredHeight);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.Default;
                    graphics.InterpolationMode = InterpolationMode.Bicubic;
                    graphics.SmoothingMode = SmoothingMode.Default;
                    graphics.PixelOffsetMode = PixelOffsetMode.Default;
                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
                // Replace the original image with the bitmap we created
                image = destImage;
            }

            // Write the image out to the publicdata area
            Console.WriteLine("[ImageUploaded] Writing image to public blob");
            var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            SaveFileToPublicBlob($"{name}.{ext}", stream);

            // Write the original path to the queue for deletion
            Console.WriteLine("[ImageUploaded] Queue for deletion");
            path = $"userdata/{name}.{ext}";
        }

        static void SaveFileToPublicBlob(string file, Stream input)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MS_AzureStorageAccountConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer publicdata = blobClient.GetContainerReference("publicdata");
            CloudBlockBlob blockBlob = publicdata.GetBlockBlobReference(file);
            blockBlob.UploadFromStream(input);
        }

        public static void ProcessDeleteQueue([QueueTrigger("delete")] string path)
        {
            Console.WriteLine($"[ProcessDeleteQueue] path = {path}");
            var connectionString = ConfigurationManager.ConnectionStrings["MS_AzureStorageAccountConnectionString"].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer userdata = blobClient.GetContainerReference("userdata");
            CloudBlockBlob blockBlob = userdata.GetBlockBlobReference(path);
            Console.WriteLine($"[ProcessDeleteQueue] deleting block blob {blockBlob.Container.Name}/{blockBlob.Name}");
            blockBlob.DeleteIfExists();
        }
    }
}
