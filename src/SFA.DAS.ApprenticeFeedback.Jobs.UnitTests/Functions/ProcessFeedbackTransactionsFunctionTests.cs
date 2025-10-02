using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Exceptions;
using SFA.DAS.ApprenticeFeedback.Jobs.Functions;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
using SFA.DAS.ApprenticeFeedback.Jobs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Functions
{
    [TestFixture]
    public class ProcessFeedbackTransactionsFunctionTests
    {
        private static ProcessFeedbackTransactionsFunction CreateSut(
            ApplicationConfiguration cfg,
            ILogger<ProcessFeedbackTransactionsFunction> logger,
            IApprenticeFeedbackApi api,
            IWaveFanoutService waveFanoutService)
            => new(cfg, logger ?? NullLogger<ProcessFeedbackTransactionsFunction>.Instance, api, waveFanoutService);

        [Test]
        public async Task Activity_CallsApi_AndReturnsResponse()
        {
            // Arrange
            var cfg = new ApplicationConfiguration { EmailPerSecondCap = 10, EmailBatchSize = 10 };
            var api = new Mock<IApprenticeFeedbackApi>(MockBehavior.Strict);
            var waveFanOutService = new Mock<IWaveFanoutService>();

            var feedbackTransaction = new FeedbackTransaction
            {
                FeedbackTransactionId = 1234
            };

            var expectedResponse = new SendApprenticeFeedbackEmailResponse
            {
                FeedbackTransactionId = 1234,
                EmailStatus = EmailStatus.Successful
            };

            api.Setup(a => a.ProcessEmailTransaction(feedbackTransaction.FeedbackTransactionId, feedbackTransaction))
               .ReturnsAsync(expectedResponse);

            var sut = CreateSut(cfg, NullLogger<ProcessFeedbackTransactionsFunction>.Instance, api.Object, waveFanOutService.Object);

            // Act
            var result = await sut.ProcessFeedbackTransactionsActivity(feedbackTransaction);

            // Assert
            Assert.That(result.FeedbackTransactionId, Is.EqualTo(1234));
            Assert.That(result.EmailStatus, Is.EqualTo(EmailStatus.Successful));
            api.VerifyAll();
        }

        [Test]
        public async Task Timer_StartsOrchestrator_WithItemsFromApi()
        {
            // Arrange
            var cfg = new ApplicationConfiguration { EmailPerSecondCap = 55, EmailBatchSize = 3 };
            var api = new Mock<IApprenticeFeedbackApi>(MockBehavior.Strict);
            var waveFanOutService = new Mock<IWaveFanoutService>();

            var items = new List<FeedbackTransaction>
            {
                new() { FeedbackTransactionId = 1 },
                new() { FeedbackTransactionId = 2 },
                new() { FeedbackTransactionId = 3 },
            };

            api.Setup(a => a.GetFeedbackTransactionsToEmail(cfg.EmailBatchSize))
               .ReturnsAsync(items);

            var fakeClient = new FakeDurableTaskClient(string.Empty) { InstanceIdToReturn = "inst-xyz" };

            var logger = new Mock<ILogger<ProcessFeedbackTransactionsFunction>>().Object;
            var sut = CreateSut(cfg, logger, api.Object, waveFanOutService.Object);

            // Act
            await sut.ProcessFeedbackTransactionsTimer(
                myTimer: null!, // unused
                orchestrationClient: fakeClient);

            // Assert
            Assert.That(fakeClient.ScheduledName, Is.EqualTo(nameof(ProcessFeedbackTransactionsFunction.ProcessFeedbackTransactionsOrchestrator)));
            var scheduled = (List<FeedbackTransaction>)fakeClient.ScheduledInput!;
            Assert.That(scheduled.Select(s => s.FeedbackTransactionId), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void Timer_ThrowsOrchestratorException_WhenApiFails()
        {
            // Arrange
            var cfg = new ApplicationConfiguration { EmailPerSecondCap = 55, EmailBatchSize = 10 };
            var api = new Mock<IApprenticeFeedbackApi>(MockBehavior.Strict);
            var waveFanOutService = new Mock<IWaveFanoutService>();

            api.Setup(a => a.GetFeedbackTransactionsToEmail(cfg.EmailBatchSize))
               .ThrowsAsync(new InvalidOperationException("boom"));

            var sut = CreateSut(cfg, NullLogger<ProcessFeedbackTransactionsFunction>.Instance, api.Object, waveFanOutService.Object);

            // Act + Assert
            var ex = Assert.ThrowsAsync<OrchestratorException>(() =>
                sut.ProcessFeedbackTransactionsTimer(myTimer: null!, orchestrationClient: null!));

            StringAssert.Contains("ProcessFeedbackTransactions orchestrator function failed.", ex!.Message);
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
        }
    }
}