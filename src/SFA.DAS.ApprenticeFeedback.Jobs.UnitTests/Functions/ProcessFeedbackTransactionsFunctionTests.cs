using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Exceptions;
using SFA.DAS.ApprenticeFeedback.Jobs.Functions;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
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
            IApprenticeFeedbackApi api)
            => new(cfg, logger ?? NullLogger<ProcessFeedbackTransactionsFunction>.Instance, api);

        [Test]
        public async Task Activity_CallsApi_AndReturnsResponse()
        {
            // Arrange
            var cfg = new ApplicationConfiguration { EmailPerSecondCap = 10, EmailBatchSize = 10 };
            var api = new Mock<IApprenticeFeedbackApi>(MockBehavior.Strict);

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

            var sut = CreateSut(cfg, NullLogger<ProcessFeedbackTransactionsFunction>.Instance, api.Object);

            // Act
            var result = await sut.ProcessFeedbackTransactionsActivity(feedbackTransaction);

            // Assert
            Assert.That(result.FeedbackTransactionId, Is.EqualTo(1234));
            Assert.That(result.EmailStatus, Is.EqualTo(EmailStatus.Successful));
            api.VerifyAll();
        }

        [Test]
        public async Task Orchestrator_FansOut_PerSecondCap_Respected()
        {
            // Arrange: 15 items, cap 10 => 10 at t0, 5 at t0+1s
            var cfg = new ApplicationConfiguration { EmailPerSecondCap = 10, EmailBatchSize = 60 };
            var api = new Mock<IApprenticeFeedbackApi>(MockBehavior.Strict); // not used by orchestrator directly

            var items = Enumerable.Range(1, 15).Select(i => new FeedbackTransaction
            {
                FeedbackTransactionId = i
            }).ToList();

            var t0 = new DateTime(2025, 9, 17, 12, 0, 0, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0, input: items)
            {
                ActivityHandler = (name, input) =>
                {
                    Assert.That(name, Is.EqualTo(nameof(ProcessFeedbackTransactionsFunction.ProcessFeedbackTransactionsActivity)));
                    var ft = (FeedbackTransaction)input!;
                    return new SendApprenticeFeedbackEmailResponse
                    {
                        FeedbackTransactionId = ft.FeedbackTransactionId,
                        EmailStatus = EmailStatus.Successful
                    };
                }
            };

            var sut = CreateSut(cfg, NullLogger<ProcessFeedbackTransactionsFunction>.Instance, api.Object);

            // Act
            var results = await sut.ProcessFeedbackTransactionsOrchestrator(ctx);

            // Assert
            Assert.That(results.Length, Is.EqualTo(15));
            Assert.That(results.Select(r => r.FeedbackTransactionId), Is.EquivalentTo(items.Select(x => x.FeedbackTransactionId)));

            // one wait because of 10/sec cap
            Assert.That(ctx.Timers.Count, Is.EqualTo(1));
            Assert.That(ctx.Timers[0], Is.EqualTo(t0.AddSeconds(1)));
        }

        [Test]
        public async Task Orchestrator_ExactlyAtCap_NoWaits()
        {
            var cfg = new ApplicationConfiguration { EmailPerSecondCap = 10, EmailBatchSize = 25 };
            var api = new Mock<IApprenticeFeedbackApi>(MockBehavior.Strict);

            var items = Enumerable.Range(1, 10).Select(i => new FeedbackTransaction
            {
                FeedbackTransactionId = i
            }).ToList();

            var t0 = new DateTime(2025, 9, 17, 12, 0, 0, DateTimeKind.Utc);
            var ctx = new FakeOrchestrationContext(t0, input: items)
            {
                ActivityHandler = (name, input) =>
                {
                    var ft = (FeedbackTransaction)input!;
                    return new SendApprenticeFeedbackEmailResponse
                    {
                        FeedbackTransactionId = ft.FeedbackTransactionId,
                        EmailStatus = EmailStatus.Successful
                    };
                }
            };

            var sut = CreateSut(cfg, NullLogger<ProcessFeedbackTransactionsFunction>.Instance, api.Object);

            var results = await sut.ProcessFeedbackTransactionsOrchestrator(ctx);

            Assert.That(results.Length, Is.EqualTo(10));
            Assert.That(ctx.Timers, Is.Empty);
        }

        [Test]
        public async Task Timer_StartsOrchestrator_WithItemsFromApi()
        {
            // Arrange
            var cfg = new ApplicationConfiguration { EmailPerSecondCap = 55, EmailBatchSize = 3 };
            var api = new Mock<IApprenticeFeedbackApi>(MockBehavior.Strict);
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
            var sut = CreateSut(cfg, logger, api.Object);

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

            api.Setup(a => a.GetFeedbackTransactionsToEmail(cfg.EmailBatchSize))
               .ThrowsAsync(new InvalidOperationException("boom"));

            var sut = CreateSut(cfg, NullLogger<ProcessFeedbackTransactionsFunction>.Instance, api.Object);

            // Act + Assert
            var ex = Assert.ThrowsAsync<OrchestratorException>(() =>
                sut.ProcessFeedbackTransactionsTimer(myTimer: null!, orchestrationClient: null!));

            StringAssert.Contains("ProcessFeedbackTransactions orchestrator function failed.", ex!.Message);
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
        }
    }
}