using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeFeedback.Jobs.Functions;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;
using SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Extensions;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Functions
{
    [TestFixture]
    public class ProcessFeedbackTargetVariantsFunctionTests
    {
        private Mock<IFeedbackTargetVariantBlobProcessor> _blobProcessorMock;
        private Mock<ILogger<ProcessFeedbackTargetVariantsFunction>> _loggerMock;
        private ProcessFeedbackTargetVariantsFunction _function;

        [SetUp]
        public void Setup()
        {
            _blobProcessorMock = new Mock<IFeedbackTargetVariantBlobProcessor>();
            _loggerMock = new Mock<ILogger<ProcessFeedbackTargetVariantsFunction>>();

            // Initialize the function with mocked dependencies
            _function = new ProcessFeedbackTargetVariantsFunction(_loggerMock.Object, _blobProcessorMock.Object);
        }

        [Test]
        public async Task VariantsTimer_ExecutesSuccessfully()
        {
            // Arrange
            var timerInfo = new TimerInfo();

            // Act
            await _function.ProcessFeedbackTargetVariantsTimer(timerInfo, _loggerMock.Object);

            // Assert
            _blobProcessorMock.Verify(x => x.ProcessBlobs(), Times.Once);
            _loggerMock.VerifyLogging(LogLevel.Information, Times.AtLeast(2));
        }

        [Test]
        public void VariantsTimer_LogsErrorOnFailure()
        {
            // Arrange
            var timerInfo = new TimerInfo();
            _blobProcessorMock
                .Setup(x => x.ProcessBlobs())
                .Returns(() => Task.FromException(new Exception("Test exception")));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _function.ProcessFeedbackTargetVariantsTimer(timerInfo, _loggerMock.Object));

            Assert.AreEqual("Test exception", ex.Message);
            _loggerMock.VerifyLoggingMessage(LogLevel.Error, Times.Once(), "ProcessFeedbackTargetVariantsTimer has failed");
        }

        [Test]
        public async Task VariantsTimer_LogsStartAndFinishMessages()
        {
            // Arrange
            var timerInfo = new TimerInfo();

            // Act
            await _function.ProcessFeedbackTargetVariantsTimer(timerInfo, _loggerMock.Object);

            // Assert
            _loggerMock.VerifyLoggingMessage(LogLevel.Information, Times.Once(), "ProcessFeedbackTargetVariantsTimer has started");
            _loggerMock.VerifyLoggingMessage(LogLevel.Information, Times.Once(), "ProcessFeedbackTargetVariantsTimer has finished");
        }
    }

}