using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Handlers
{
    public class ApprenticeshipConfirmationConfirmedEventHandlerTests
    {
        private Mock<IApprenticeFeedbackApi> _mockApi;
        private Mock<ILogger<ApprenticeshipConfirmationConfirmedEventHandler>> _mockLogger;
        private TestableMessageHandlerContext _messageHandlerContext;
        private ApprenticeshipConfirmationConfirmedEventHandler _handler;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockApi = new Mock<IApprenticeFeedbackApi>();
            _mockLogger = new Mock<ILogger<ApprenticeshipConfirmationConfirmedEventHandler>>();
            _messageHandlerContext = new TestableMessageHandlerContext();
            _handler = new ApprenticeshipConfirmationConfirmedEventHandler(_mockApi.Object, _mockLogger.Object);
        }

        [Test, AutoMoqData]
        public async Task Handle_ShouldCallApi_WhenMessageIsReceived()
        {
            var message = _fixture.Build<ApprenticeshipConfirmationConfirmedEvent>()
               .Create();

            await _handler.Handle(message, _messageHandlerContext);

            _mockApi.Verify(m => m.CreateFeedbackTarget(It.Is<ApprenticeConfirmedDetails>(n =>
                n.ApprenticeId == message.ApprenticeId &&
                n.ApprenticeshipId == message.ApprenticeshipId &&
                n.CommitmentsApprenticeshipId == message.CommitmentsApprenticeshipId &&
                n.CommitmentsApprovedOn == message.CommitmentsApprovedOn &&
                n.ConfirmationId == message.ConfirmationId &&
                n.ConfirmedOn == message.ConfirmedOn)));
        }
    }
}