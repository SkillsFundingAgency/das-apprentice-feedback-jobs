using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace SFA.DAS.ApprenticeFeedback.Jobs.Helpers
{
    public class BlobStorageHelper : IBlobStorageHelper
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageHelper(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task MoveBlobAsync(string containerName, string sourceBlobPath, string destinationBlobPath)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var sourceBlobClient = containerClient.GetBlobClient(sourceBlobPath);

            if (!await sourceBlobClient.ExistsAsync())
            {
                throw new Exception($"Source blob '{sourceBlobPath}' does not exist.");
            }

            var destinationBlobClient = containerClient.GetBlobClient(destinationBlobPath);

            var copyOperation = await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);
            var copyInfo = copyOperation.Value;  

            BlobProperties properties = await destinationBlobClient.GetPropertiesAsync();

            while (properties.CopyStatus == CopyStatus.Pending)
            {
                await Task.Delay(500); 
                properties = await destinationBlobClient.GetPropertiesAsync();
            }

            if (properties.CopyStatus == CopyStatus.Success)
            {
                await sourceBlobClient.DeleteAsync();
                Console.WriteLine($"Blob moved from {sourceBlobPath} to {destinationBlobPath} successfully.");
            }
            else
            {
                throw new Exception($"Failed to move blob. Copy status: {properties.CopyStatus}");
            }
        }
    }
}