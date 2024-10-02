using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions.UnitTests;

[TestFixture]
public class ProcessFeedbackTargetVariantsFunctionTests
{
    private Mock<ILogger<ProcessFeedbackTargetVariantsFunction>> _loggerMock;
    private Mock<IApprenticeFeedbackApi> _apiMock;
    private Mock<IBlobStorageHelper> _blobStorageHelperMock;
    private ApprenticeFeedbackVariantConfiguration _config;
    private ProcessFeedbackTargetVariantsFunction _function;

    [SetUp]
    public void SetUp()
    {
        _apiMock = new Mock<IApprenticeFeedbackApi>();
        _loggerMock = new Mock<ILogger<ProcessFeedbackTargetVariantsFunction>>();
        _blobStorageHelperMock = new Mock<IBlobStorageHelper>();
        _config = new ApprenticeFeedbackVariantConfiguration 
        {
            BlobContainerName = "test-container",
            IncomingFolder = "new",
            ArchiveFolder = "archive"
        };

        _function = new ProcessFeedbackTargetVariantsFunction(_loggerMock.Object, _apiMock.Object, _blobStorageHelperMock.Object, _config;
    }

    [Test]
    public async Task Run_ShouldProcessValidCsvAndMoveBlob()
    {
        // Arrange
        var validCsv = "ApprenticeshipId,Variant\n1001,A\n1002,B\n";
        var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(validCsv));
        var commandName = "test.csv";

        _apiMock
            .Setup(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()))
            .Returns(Task.CompletedTask);

        _blobStorageHelperMock
            .Setup(x => x.MoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.Run(blobStream, commandName);

        // Assert
        _apiMock.Verify(x => x.ProcessFeedbackTargetVariants(It.Is<PostProcessFeedbackVariantsRequest>(
            variants => variants.FeedbackVariants.Count == 2)), Times.Once);
        _blobStorageHelperMock.Verify(x => x.MoveBlobAsync(_config.BlobContainerName, _config.IncomingFolder, _config.ArchiveFolder), Times.Once);
    }

    [Test]
    public async Task Run_ShouldLogWarningsForInvalidRow_AndProcessRemainingRows()
    {
        // Arrange
        var invalidCsv = "ApprenticeshipId,Variant\n1001,A\nInvalidRow\n";
        var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(invalidCsv));
        var commandName = "test.csv";

        _apiMock
            .Setup(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()))
            .Returns(Task.CompletedTask);

        _blobStorageHelperMock
            .Setup(x => x.MoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.Run(blobStream, commandName);

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("Invalid row in file"))));

        _apiMock.Verify(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()), Times.Once);
        _blobStorageHelperMock.Verify(x => x.MoveBlobAsync(_config.BlobContainerName, _config.IncomingFolder, _config.ArchiveFolder), Times.Once);
    }

    [Test]
    public async Task Run_ShouldLogWarningsForInvalidApprenticeshipId_AndProcessRemainingRows()
    {
        // Arrange
        var invalidCsv = "ApprenticeshipId,Variant\nXYZ,A\n1002,B\n"; 
        var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(invalidCsv));
        var commandName = "test.csv";

        _apiMock
            .Setup(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()))
            .Returns(Task.CompletedTask);

        _blobStorageHelperMock
            .Setup(x => x.MoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.Run(blobStream, commandName);

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("Invalid ApprenticeshipId in row"))));

        _apiMock.Verify(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()), Times.Once);
        _blobStorageHelperMock.Verify(x => x.MoveBlobAsync(_config.BlobContainerName, _config.IncomingFolder, _config.ArchiveFolder), Times.Once);
    }

    [Test]
    public async Task Run_ShouldLogWarningsForInvalidVariant_AndProcessRemainingRows()
    {
        // Arrange
        var invalidCsv = "ApprenticeshipId,Variant\n1001,A\n1002,\n";
        var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(invalidCsv));
        var commandName = "test.csv";

        _apiMock
            .Setup(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()))
            .Returns(Task.CompletedTask);

        _blobStorageHelperMock
            .Setup(x => x.MoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.Run(blobStream, commandName);

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.Is<string>(s => s.Contains("Empty Variant in row"))));

        _apiMock.Verify(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()), Times.Once);
        _blobStorageHelperMock.Verify(x => x.MoveBlobAsync(_config.BlobContainerName, _config.IncomingFolder, _config.ArchiveFolder), Times.Once);
    }

    [Test]
    public async Task Run_ShouldShouldMoveEmptyFile()
    {
        // Arrange
        var invalidCsv = "";
        var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(invalidCsv));
        var commandName = "test.csv";

        _blobStorageHelperMock
            .Setup(x => x.MoveBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _function.Run(blobStream, commandName);

        // Assert
        _apiMock.Verify(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()), Times.Never);
        _blobStorageHelperMock.Verify(x => x.MoveBlobAsync(_config.BlobContainerName, _config.IncomingFolder, _config.ArchiveFolder), Times.Once);
    }


}
