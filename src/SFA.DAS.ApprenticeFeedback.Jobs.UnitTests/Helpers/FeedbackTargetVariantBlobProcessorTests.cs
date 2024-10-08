using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Helpers
{
    [TestFixture]
    public class FeedbackTargetVariantBlobProcessorTests
    {
        private Mock<ILogger<FeedbackTargetVariantBlobProcessor>> _mockLogger;
        private Mock<BlobServiceClient> _mockBlobServiceClient;
        private Mock<IOptions<FeedbackTargetVariantConfiguration>> _mockConfig;
        private Mock<IFeedbackTargetVariantBlobReader> _mockBlobReader;
        private Mock<IFeedbackTargetVariantBatchProcessor> _mockBatchProcessor;
        private Mock<IFeedbackTargetVariantBlobMover> _mockBlobMover;
        private FeedbackTargetVariantBlobProcessor _processor;
        private FeedbackTargetVariantConfiguration _config;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<FeedbackTargetVariantBlobProcessor>>();
            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockConfig = new Mock<IOptions<FeedbackTargetVariantConfiguration>>();
            _mockBlobReader = new Mock<IFeedbackTargetVariantBlobReader>();
            _mockBatchProcessor = new Mock<IFeedbackTargetVariantBatchProcessor>();
            _mockBlobMover = new Mock<IFeedbackTargetVariantBlobMover>();

            _config = new FeedbackTargetVariantConfiguration
            {
                BlobContainerName = "test-container",
                ArchiveFolder = "archive",
                FeedbackTargetVariantBatchSize = 10
            };
            _mockConfig.Setup(c => c.Value).Returns(_config);

            _processor = new FeedbackTargetVariantBlobProcessor(
                _mockLogger.Object,
                _mockBlobServiceClient.Object,
                _mockConfig.Object,
                _mockBlobReader.Object,
                _mockBlobMover.Object,
                _mockBatchProcessor.Object
            );
        }

        [Test]
        public async Task ProcessBlobs_Should_Process_And_Move_Blob_When_Variants_Are_Found()
        {
            // Arrange
            var blobItems = new List<BlobItem> { BlobsModelFactory.BlobItem("blob1.csv") };
            var blobContainerClient = new Mock<BlobContainerClient>();

            _mockBlobServiceClient
                .Setup(b => b.GetBlobContainerClient(_config.BlobContainerName))
                .Returns(blobContainerClient.Object);

            blobContainerClient
                .Setup(c => c.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncPageable<BlobItem>.FromPages(new[] { Page<BlobItem>.FromValues(blobItems, null, null) }));

            var blobClient = new Mock<BlobClient>();
            blobContainerClient
                .Setup(c => c.GetBlobClient("blob1.csv"))
                .Returns(blobClient.Object);

            var feedbackVariants = new List<FeedbackVariant>
        {
            new FeedbackVariant(1, "variant1"),
            new FeedbackVariant(2, "variant2")
        };

            _mockBlobReader
                .Setup(r => r.ReadVariantsFromBlob(blobClient.Object))
                .ReturnsAsync(feedbackVariants);

            // Act
            await _processor.ProcessBlobs();

            // Assert
            _mockBlobReader.Verify(r => r.ReadVariantsFromBlob(blobClient.Object), Times.Once);
            _mockBatchProcessor.Verify(b => b.ProcessBatch(feedbackVariants, _config.FeedbackTargetVariantBatchSize), Times.Once);
            _mockBlobMover.Verify(m => m.MoveBlob(_config.BlobContainerName, "blob1.csv", _config.ArchiveFolder), Times.Once);
        }

        [Test]
        public async Task ProcessBlobs_Should_Not_Process_When_No_Variants_Are_Found()
        {
            // Arrange
            var blobItems = new List<BlobItem> { BlobsModelFactory.BlobItem("blob1.csv") };
            var blobContainerClient = new Mock<BlobContainerClient>();

            _mockBlobServiceClient
                .Setup(b => b.GetBlobContainerClient(_config.BlobContainerName))
                .Returns(blobContainerClient.Object);

            blobContainerClient
                .Setup(c => c.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncPageable<BlobItem>.FromPages(new[] { Page<BlobItem>.FromValues(blobItems, null, null) }));

            var blobClient = new Mock<BlobClient>();
            blobContainerClient
                .Setup(c => c.GetBlobClient("blob1.csv"))
                .Returns(blobClient.Object);

            _mockBlobReader
                .Setup(r => r.ReadVariantsFromBlob(blobClient.Object))
                .ReturnsAsync(new List<FeedbackVariant>());

            // Act
            await _processor.ProcessBlobs();

            // Assert
            _mockBlobReader.Verify(r => r.ReadVariantsFromBlob(blobClient.Object), Times.Once);
            _mockBatchProcessor.Verify(b => b.ProcessBatch(It.IsAny<List<FeedbackVariant>>(), It.IsAny<int>()), Times.Never);
            _mockBlobMover.Verify(m => m.MoveBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ProcessBlobs_Should_Not_Process_If_Blob_Has_Subfolder()
        {
            // Arrange
            var blobItems = new List<BlobItem> { BlobsModelFactory.BlobItem("folder/blob1.csv") };
            var blobContainerClient = new Mock<BlobContainerClient>();

            _mockBlobServiceClient
                .Setup(b => b.GetBlobContainerClient(_config.BlobContainerName))
                .Returns(blobContainerClient.Object);

            blobContainerClient
                .Setup(c => c.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncPageable<BlobItem>.FromPages(new[] { Page<BlobItem>.FromValues(blobItems, null, null) }));

            // Act
            await _processor.ProcessBlobs();

            // Assert
            _mockBlobReader.Verify(r => r.ReadVariantsFromBlob(It.IsAny<BlobClient>()), Times.Never);
            _mockBatchProcessor.Verify(b => b.ProcessBatch(It.IsAny<List<FeedbackVariant>>(), It.IsAny<int>()), Times.Never);
            _mockBlobMover.Verify(m => m.MoveBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }

}