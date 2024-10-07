using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants
{
    public interface IFeedbackTargetVariantBlobProcessor
    {
        Task ProcessBlobs();
    }
    public class FeedbackTargetVariantBlobProcessor : IFeedbackTargetVariantBlobProcessor
    {
        private readonly ILogger<FeedbackTargetVariantBlobProcessor> _logger;
        private readonly FeedbackTargetVariantConfiguration _config;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IFeedbackTargetVariantBlobReader _blobReader;
        private readonly IFeedbackTargetVariantBlobMover _blobMover;
        private readonly IFeedbackTargetVariantBatchProcessor _batchProcessor;

        public FeedbackTargetVariantBlobProcessor(
            ILogger<FeedbackTargetVariantBlobProcessor> logger,
            BlobServiceClient blobServiceClient,
            IOptions<FeedbackTargetVariantConfiguration> config,
            IFeedbackTargetVariantBlobReader blobReader,
            IFeedbackTargetVariantBlobMover blobMover,
            IFeedbackTargetVariantBatchProcessor batchProcessor
            )
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _config = config.Value;
            _blobReader = blobReader;
            _blobMover = blobMover;
            _batchProcessor = batchProcessor;
        }

        public async Task ProcessBlobs()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_config.BlobContainerName);
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                string blobName = blobItem.Name;
                if (blobName.Contains("/"))
                {
                    continue;
                }
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                _logger.LogInformation($"Processing blob: {blobName}");

                var variants = await _blobReader.ReadVariantsFromBlob(blobClient);

                if (variants.Any())
                {
                    await _batchProcessor.ProcessBatch(variants, _config.FeedbackTargetVariantBatchSize);
                }

                await _blobMover.MoveBlob(_config.BlobContainerName, blobName, _config.ArchiveFolder);
                _logger.LogInformation($"Blob {blobName} moved to archive.");
            }
        }
    }

}
