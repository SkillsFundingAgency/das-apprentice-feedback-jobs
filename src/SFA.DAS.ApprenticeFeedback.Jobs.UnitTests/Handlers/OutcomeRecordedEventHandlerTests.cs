using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Handlers.AssessorHandlers;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using SFA.DAS.AssessorService.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Handlers
{
    [TestFixture]
    public class OutcomeRecordedEventHandlerTests
    {
        private Mock<IApprenticeFeedbackApi> _apprenticeFeedbackApi;
        private Mock<ILogger<OutcomeRecordedEventHandler>> _logger;
        private TestableMessageHandlerContext _messageHandlerContext;
        private OutcomeRecordedEventHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _apprenticeFeedbackApi = new Mock<IApprenticeFeedbackApi>();
            _logger = new Mock<ILogger<OutcomeRecordedEventHandler>>();
            _messageHandlerContext = new TestableMessageHandlerContext();
            _handler = new OutcomeRecordedEventHandler(_apprenticeFeedbackApi.Object, _logger.Object);
        }

        private static OutcomeRecordedEvent CreateEvent(OutcomeType outcome) => new OutcomeRecordedEvent
        {
            ApprenticeshipId = 1159393,
            AchievementDate = DateTime.UtcNow,
            Outcome = outcome,
            StandardName = "Standard",
            CertificateId = Guid.NewGuid()
        };

        [Test]
        public async Task Handle_ShouldCreateFeedbackTransaction_WhenOutcomeIsPass()
        {
            var message = CreateEvent(OutcomeType.Pass);

            await _handler.Handle(message, _messageHandlerContext);

            _apprenticeFeedbackApi.Verify(api => api.CreateFeedbackTransaction(
                It.Is<CreateFeedbackTransactionRequest>(r =>
                    r.ApprenticeshipId == message.ApprenticeshipId &&
                    r.TemplateName == "ApprenticeshipOutcome")),
                Times.Once);
        }

        [Test]
        public async Task Handle_ShouldNotCallApi_WhenOutcomeIsFail()
        {
            await _handler.Handle(CreateEvent(OutcomeType.Fail), _messageHandlerContext);

            _apprenticeFeedbackApi.Verify(api => api.CreateFeedbackTransaction(It.IsAny<CreateFeedbackTransactionRequest>()), Times.Never);
        }

        [Test]
        public void Handle_ShouldRethrow_WhenApiFails()
        {
            _apprenticeFeedbackApi.Setup(api => api.CreateFeedbackTransaction(It.IsAny<CreateFeedbackTransactionRequest>()))
                .Throws(new InvalidOperationException("boom"));


            Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(CreateEvent(OutcomeType.Pass), _messageHandlerContext));
        }
    }
}
