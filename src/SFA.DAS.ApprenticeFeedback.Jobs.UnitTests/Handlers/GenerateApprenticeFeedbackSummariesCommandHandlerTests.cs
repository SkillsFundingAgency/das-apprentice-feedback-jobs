using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Messages.Commands;
using SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Handlers
{
    public class GenerateApprenticeFeedbackSummariesCommandHandlerTests
    {
        private Mock<IApprenticeFeedbackApi> _mockApi;
        private Mock<ILogger<GenerateApprenticeFeedbackSummariesCommandHandler>> _mockLogger;
        private TestableMessageHandlerContext _messageHandlerContext;
        private GenerateApprenticeFeedbackSummariesCommandHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockApi = new Mock<IApprenticeFeedbackApi>();
            _mockLogger = new Mock<ILogger<GenerateApprenticeFeedbackSummariesCommandHandler>>();
            _messageHandlerContext = new TestableMessageHandlerContext();
            _handler = new GenerateApprenticeFeedbackSummariesCommandHandler(_mockApi.Object, _mockLogger.Object);
        }

        [Test, AutoMoqData]
        public async Task Handle_ShouldCallApi_WhenMessageIsReceived()
        {
            // Arrange
            var command = new GenerateApprenticeFeedbackSummariesCommand();

            // Act
            await _handler.Handle(command, _messageHandlerContext);

            // Assert
            _mockApi.Verify(m => m.GenerateFeedbackSummaries(), Times.Once);
        }
    }
}
