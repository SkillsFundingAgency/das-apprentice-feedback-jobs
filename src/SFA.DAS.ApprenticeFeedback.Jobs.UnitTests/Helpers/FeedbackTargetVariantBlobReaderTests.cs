using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;
using SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Extensions;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Helpers
{
    [TestFixture]
    public class FeedbackTargetVariantBlobReaderTests
    {
        private Mock<ILogger<FeedbackTargetVariantBlobReader>> _mockLogger;
        private Mock<BlobClient> _mockBlobClient;
        private FeedbackTargetVariantBlobReader _blobReader;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<FeedbackTargetVariantBlobReader>>();
            _mockBlobClient = new Mock<BlobClient>();
            _blobReader = new FeedbackTargetVariantBlobReader(_mockLogger.Object);
        }

        [Test]
        public async Task ReadVariantsFromBlob_Should_Return_FeedbackVariants_When_BlobData_Is_Valid()
        {
            // Arrange
            var blobData = "ApprenticeshipId,Variant\n12345,Variant1\n67890,Variant2";
            var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));

            _mockBlobClient
                .Setup(x => x.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blobStream);

            // Act
            var result = await _blobReader.ReadVariantsFromBlob(_mockBlobClient.Object);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainSingle(v => v.ApprenticeshipId == 12345 && v.Variant == "Variant1");
            result.Should().ContainSingle(v => v.ApprenticeshipId == 67890 && v.Variant == "Variant2");
        }

        [Test]
        public async Task ReadVariantsFromBlob_Should_Return_Empty_List_When_Blob_Has_Header_Only()
        {
            // Arrange
            var blobData = "ApprenticeshipId,Variant\n";  // Header only, no data rows
            var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));

            _mockBlobClient
                .Setup(x => x.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blobStream);

            // Act
            var result = await _blobReader.ReadVariantsFromBlob(_mockBlobClient.Object);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ReadVariantsFromBlob_Should_Log_Warning_For_Invalid_ApprenticeshipId()
        {
            // Arrange
            var blobData = "ApprenticeshipId,Variant\nInvalidId,Variant1";
            var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));

            _mockBlobClient
                .Setup(x => x.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blobStream);

            // Act
            var result = await _blobReader.ReadVariantsFromBlob(_mockBlobClient.Object);

            // Assert
            result.Should().BeEmpty();

            _mockLogger.VerifyLoggingMessage(LogLevel.Warning, Times.Once(), "Invalid ApprenticeshipId in row");
        }

        [Test]
        public async Task ReadVariantsFromBlob_Should_Accept_Empty_Variant()
        {
            // Arrange
            var blobData = "ApprenticeshipId,Variant\n12345,";  // ApprenticeshipId provided, but no Variant
            var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));

            _mockBlobClient
                .Setup(x => x.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blobStream);

            // Act
            var result = await _blobReader.ReadVariantsFromBlob(_mockBlobClient.Object);

            // Assert
            result.Should().HaveCount(1);

            result.Should().ContainSingle(v => v.ApprenticeshipId == 12345 && v.Variant == null);
        }

        [Test]
        public async Task ReadVariantsFromBlob_Should_Log_Warning_For_Invalid_Row_Format()
        {
            // Arrange
            var blobData = "ApprenticeshipId,Variant\n12345";  // Missing Variant column
            var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(blobData));

            _mockBlobClient
                .Setup(x => x.OpenReadAsync(It.IsAny<long>(), It.IsAny<int?>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(blobStream);

            // Act
            var result = await _blobReader.ReadVariantsFromBlob(_mockBlobClient.Object);

            // Assert
            result.Should().BeEmpty();

            _mockLogger.VerifyLoggingMessage(LogLevel.Warning, Times.Once(), "Invalid row in file");
        }
    }
}
