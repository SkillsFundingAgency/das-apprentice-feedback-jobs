using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants
{
    public interface IFeedbackTargetVariantBlobMover
    {
        Task MoveBlob(string containerName, string blobName, string destinationFolder);
    }

    public class FeedbackTargetVariantBlobMover : IFeedbackTargetVariantBlobMover
    {
        private readonly ILogger<FeedbackTargetVariantBlobMover> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public FeedbackTargetVariantBlobMover(ILogger<FeedbackTargetVariantBlobMover> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }

        public async Task MoveBlob(string containerName, string blobName, string destinationFolder)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var sourceBlobClient = containerClient.GetBlobClient(blobName);

            if (!await sourceBlobClient.ExistsAsync())
            {
                throw new Exception($"Source blob '{blobName}' does not exist.");
            }

            var destinationBlobPath = !string.IsNullOrWhiteSpace(destinationFolder)
                ? $"{destinationFolder}/{blobName}"
                : blobName;

            var destinationBlobClient = containerClient.GetBlobClient(destinationBlobPath);

            var copyOptions = new BlobCopyFromUriOptions();
            await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri, copyOptions);

            BlobProperties properties;
            do
            {
                await Task.Delay(500);
                properties = await destinationBlobClient.GetPropertiesAsync();
            } while (properties.CopyStatus == CopyStatus.Pending);

            if (properties.CopyStatus == CopyStatus.Success)
            {
                await sourceBlobClient.DeleteAsync();
                _logger.LogInformation($"{blobName} moved from {containerName} to {destinationBlobPath} successfully.");
            }
            else
            {
                throw new Exception($"Failed to move blob. Copy status: {properties.CopyStatus}");
            }
        }
    }

}
