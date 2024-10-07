using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Helpers
{
    [TestFixture]
    public class FeedbackTargetVariantBatchProcessorTests
    {
        private Mock<ILogger<FeedbackTargetVariantBatchProcessor>> _loggerMock;
        private Mock<IApprenticeFeedbackApi> _apprenticeFeedbackApiMock;
        private FeedbackTargetVariantBatchProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<FeedbackTargetVariantBatchProcessor>>();
            _apprenticeFeedbackApiMock = new Mock<IApprenticeFeedbackApi>();
            _processor = new FeedbackTargetVariantBatchProcessor(_loggerMock.Object, _apprenticeFeedbackApiMock.Object);
        }

        [Test]
        public async Task ProcessBatch_SendsCorrectBatches()
        {
            // Arrange
            var feedbackVariants = new List<FeedbackVariant>
            {
                new FeedbackVariant(123465, "A"),
                new FeedbackVariant(24687, "B"),
                new FeedbackVariant(13579, "C"),
                new FeedbackVariant(98765, "A"),
                new FeedbackVariant(64978, "B")
            };
            int batchSize = 2;

            var requests = new List<PostProcessFeedbackVariantsRequest>();
            _apprenticeFeedbackApiMock
                .Setup(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()))
                .Returns(Task.CompletedTask)
                .Callback<PostProcessFeedbackVariantsRequest>(req => requests.Add(req));

            // Act
            await _processor.ProcessBatch(feedbackVariants, batchSize);

            // Assert
            requests.Should().HaveCount(3);

            // First batch
            requests[0].FeedbackTargetVariants.Should().HaveCount(2);
            requests[0].ClearStaging.Should().BeTrue();
            requests[0].MergeStaging.Should().BeFalse();

            // Second batch
            requests[1].FeedbackTargetVariants.Should().HaveCount(2);
            requests[1].ClearStaging.Should().BeFalse();
            requests[1].MergeStaging.Should().BeFalse();

            // Third batch
            requests[2].FeedbackTargetVariants.Should().HaveCount(1);
            requests[2].ClearStaging.Should().BeFalse();
            requests[2].MergeStaging.Should().BeTrue();
        }

        [Test]
        public async Task ProcessBatch_WithBatchSizeEqualToVariantsCount_SendsSingleBatch()
        {
            // Arrange
            var feedbackVariants = new List<FeedbackVariant>
            {
                new FeedbackVariant(123465, "A"),
                new FeedbackVariant(24687, "B"),
                new FeedbackVariant(13579, "C"),
                new FeedbackVariant(98765, "A")
            };
            int batchSize = 4;

            var requests = new List<PostProcessFeedbackVariantsRequest>();
            _apprenticeFeedbackApiMock
                .Setup(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()))
                .Returns(Task.CompletedTask)
                .Callback<PostProcessFeedbackVariantsRequest>(req => requests.Add(req));

            // Act
            await _processor.ProcessBatch(feedbackVariants, batchSize);

            // Assert
            requests.Should().HaveCount(1);
            requests[0].FeedbackTargetVariants.Should().HaveCount(4);
            requests[0].ClearStaging.Should().BeTrue();
            requests[0].MergeStaging.Should().BeTrue();
        }

        [Test]
        public async Task ProcessBatch_WithEmptyVariants_DoesNotSendAnyBatches()
        {
            // Arrange
            var feedbackVariants = new List<FeedbackVariant>();
            int batchSize = 2;

            var requests = new List<PostProcessFeedbackVariantsRequest>();
            _apprenticeFeedbackApiMock
                .Setup(x => x.ProcessFeedbackTargetVariants(It.IsAny<PostProcessFeedbackVariantsRequest>()))
                .Returns(Task.CompletedTask)
                .Callback<PostProcessFeedbackVariantsRequest>(req => requests.Add(req));

            // Act
            await _processor.ProcessBatch(feedbackVariants, batchSize);

            // Assert
            requests.Should().BeEmpty();
        }

        [Test]
        public async Task ProcessBatch_LogsInformationForEachBatch()
        {
            // Arrange
            var feedbackVariants = new List<FeedbackVariant>
            {
                new FeedbackVariant(123465, "A"),
                new FeedbackVariant(24687, "B"),
                new FeedbackVariant(13579, "C"),
            };
            int batchSize = 2;

            // Act
            await _processor.ProcessBatch(feedbackVariants, batchSize);

            // Assert
            _loggerMock.VerifyLoggingMessage(LogLevel.Information, Times.Once(), "Processed batch with ClearStaging: True, MergeStaging: False");
            _loggerMock.VerifyLoggingMessage(LogLevel.Information, Times.Once(), "Processed batch with ClearStaging: False, MergeStaging: True");
        }
    }
}
