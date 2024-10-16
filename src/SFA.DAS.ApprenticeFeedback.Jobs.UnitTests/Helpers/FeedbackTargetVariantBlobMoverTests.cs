using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;
using SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Helpers
{
    [TestFixture]
    public class FeedbackTargetVariantBlobMoverTests
    {
        private Mock<ILogger<FeedbackTargetVariantBlobMover>> _loggerMock;
        private Mock<BlobServiceClient> _blobServiceClientMock;
        private Mock<BlobContainerClient> _blobContainerClientMock;
        private Mock<BlobClient> _sourceBlobClientMock;
        private Mock<BlobClient> _destinationBlobClientMock;
        private FeedbackTargetVariantBlobMover _blobMover;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<FeedbackTargetVariantBlobMover>>();
            _blobServiceClientMock = new Mock<BlobServiceClient>();
            _blobContainerClientMock = new Mock<BlobContainerClient>();
            _sourceBlobClientMock = new Mock<BlobClient>();
            _destinationBlobClientMock = new Mock<BlobClient>();

            _blobServiceClientMock
                .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_blobContainerClientMock.Object);

            _blobContainerClientMock
                .Setup(x => x.GetBlobClient(It.IsAny<string>()))
                .Returns<string>(blobName =>
                {
                    if (blobName.Contains("destination-folder"))
                        return _destinationBlobClientMock.Object;
                    else
                        return _sourceBlobClientMock.Object;
                });

            _blobMover = new FeedbackTargetVariantBlobMover(_loggerMock.Object, _blobServiceClientMock.Object);
        }

        [Test]
        public async Task MoveBlob_WhenSourceBlobDoesNotExist_ThrowsException()
        {
            // Arrange
            string containerName = "test-container";
            string blobName = "test-blob";
            string destinationFolder = "destination-folder";

            _sourceBlobClientMock.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            // Act
            Func<Task> act = async () => await _blobMover.MoveBlob(containerName, blobName, destinationFolder);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage($"Source blob '{blobName}' does not exist.");
        }

        [Test]
        public async Task MoveBlob_WhenCopySucceeds_DeletesSourceBlobAndLogsInformation()
        {
            // Arrange
            string containerName = "test-container";
            string blobName = "test-blob";
            string destinationFolder = "destination-folder";

            var sourceUri = new Uri($"https://{containerName}.blob.core.windows.net/{containerName}/{blobName}");
            _sourceBlobClientMock.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            _sourceBlobClientMock.SetupGet(x => x.Uri).Returns(sourceUri);
            
            var properties = GetBlobProperties(CopyStatus.Success);
            _destinationBlobClientMock
                .Setup(c => c.GetPropertiesAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(properties, null));

            // Act
            await _blobMover.MoveBlob(containerName, blobName, destinationFolder);

            // Assert
            _destinationBlobClientMock.Verify(x => x.StartCopyFromUriAsync(
                sourceUri,
                It.IsAny<BlobCopyFromUriOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);

            _sourceBlobClientMock.Verify(x => x.DeleteAsync(DeleteSnapshotsOption.None, null, It.IsAny<CancellationToken>()), Times.Once);

            _loggerMock.VerifyLoggingMessage(LogLevel.Information, Times.Once(), $"{blobName} moved from {containerName} to {destinationFolder}/{blobName} successfully.");
        }

        [Test]
        public async Task MoveBlob_WhenCopyFails_ThrowsException()
        {
            // Arrange
            string containerName = "test-container";
            string blobName = "test-blob";
            string destinationFolder = "destination-folder";

            _sourceBlobClientMock.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            var copyStatusSequence = new Queue<CopyStatus>(new[] { CopyStatus.Pending, CopyStatus.Failed });
            _destinationBlobClientMock.Setup(x => x.GetPropertiesAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    var status = copyStatusSequence.Dequeue();
                    var properties = GetBlobProperties(status);
                    return Response.FromValue(properties, Mock.Of<Response>());
                });

            // Act
            Func<Task> act = async () => await _blobMover.MoveBlob(containerName, blobName, destinationFolder);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage($"Failed to move blob. Copy status: {CopyStatus.Failed}");
        }

        private BlobProperties GetBlobProperties(CopyStatus copyStatus)
        {
            return BlobsModelFactory.BlobProperties(
                    lastModified: DateTimeOffset.UtcNow,
                    leaseStatus: LeaseStatus.Unlocked,
                    contentLength: 1024L,
                    contentType: "application/octet-stream",
                    eTag: new ETag("0x8D7C2B3A4B5C6D7E"),
                    leaseState: LeaseState.Available,
                    contentEncoding: "gzip",
                    contentDisposition: "inline",
                    contentLanguage: "en-US",
                    cacheControl: "no-cache",
                    blobSequenceNumber: 0L,
                    leaseDuration: LeaseDurationType.Infinite,
                    acceptRanges: "bytes",
                    destinationSnapshot: null,
                    blobCommittedBlockCount: 0,
                    isIncrementalCopy: false,
                    isServerEncrypted: true,
                    blobCopyStatus: copyStatus,
                    encryptionKeySha256: null,
                    copySource: null,
                    encryptionScope: null,
                    copyProgress: null,
                    accessTier: "Hot",
                    copyId: null,
                    accessTierInferred: false,
                    copyStatusDescription: null,
                    archiveStatus: null,
                    copyCompletedOn: DateTimeOffset.UtcNow,
                    accessTierChangedOn: DateTimeOffset.UtcNow,
                    blobType: BlobType.Block,
                    versionId: null,
                    objectReplicationSourceProperties: null,
                    isLatestVersion: true,
                    objectReplicationDestinationPolicyId: null,
                    tagCount: 0L,
                    metadata: null,
                    expiresOn: DateTimeOffset.UtcNow.AddYears(1),
                    createdOn: DateTimeOffset.UtcNow,
                    isSealed: false,
                    rehydratePriority: null,
                    contentHash: null,
                    lastAccessed: DateTimeOffset.UtcNow,
                    immutabilityPolicy: null,
                    hasLegalHold: false
                );
        }
    }
}
