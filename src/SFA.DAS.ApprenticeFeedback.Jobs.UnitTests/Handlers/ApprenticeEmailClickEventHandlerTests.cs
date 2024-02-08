using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeFeedbackHandlers;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Extensions;
using SFA.DAS.ApprenticeFeedback.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests.Handlers
{
    [TestFixture]
    public class ApprenticeEmailClickEventHandlerTests
    {
        private Mock<IApprenticeFeedbackApi> _apprenticeFeedbackApi;
        private Mock<ILogger<ApprenticeEmailClickEventHandler>> _logger;
        private TestableMessageHandlerContext _messageHandlerContext;
        private ApprenticeEmailClickEventHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _apprenticeFeedbackApi = new Mock<IApprenticeFeedbackApi>();
            _logger = new Mock<ILogger<ApprenticeEmailClickEventHandler>>();
            _messageHandlerContext = new TestableMessageHandlerContext();
            _handler = new ApprenticeEmailClickEventHandler(_apprenticeFeedbackApi.Object, _logger.Object);
        }

        [Test]
        public async Task Handle_ShouldCallApi_WhenMessageIsReceived()
        {
            // Arrange
            var message = new ApprenticeEmailClickEvent
            {
                FeedbackTransactionId = 101,
                ApprenticeFeedbackTargetId = Guid.NewGuid(),
                Linkname = "SomeLinkName",
                Link = "https://somewhere.com",
                ClickedOn = DateTime.Now
            };

            // Act
            await _handler.Handle(message, _messageHandlerContext);

            // Assert
            _apprenticeFeedbackApi.Verify(api => api.TrackFeedbackTransactionClick(
                It.Is<long>(p => p == message.FeedbackTransactionId),
                It.Is<FeedbackTransactionClickRequest>(p =>
                    p.FeedbackTransactionId == message.FeedbackTransactionId &&
                    p.ApprenticeFeedbackTargetId == message.ApprenticeFeedbackTargetId &&
                    p.LinkName == message.Linkname &&
                    p.LinkUrl == message.Link &&
                    p.ClickedOn == message.ClickedOn)),
                Times.Once);
        }

        [Test]
        public async Task Handle_ShouldLogInformation_WhenMessageIsReceived()
        {
            // Arrange
            var message = new ApprenticeEmailClickEvent
            {
                FeedbackTransactionId = 101,
                ApprenticeFeedbackTargetId = Guid.NewGuid(),
                Linkname = "SomeLinkName",
                Link = "https://somewhere.com",
                ClickedOn = DateTime.Now
            };

            // Act
            await _handler.Handle(message, _messageHandlerContext);

            // Assert
            _logger.VerifyLogging(LogLevel.Debug, Times.Once());
        }

        [Test]
        public void Handle_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var message = new ApprenticeEmailClickEvent
            {
                FeedbackTransactionId = 101,
                ApprenticeFeedbackTargetId = Guid.NewGuid(),
                Linkname = "SomeLinkName",
                Link = "https://somewhere.com",
                ClickedOn = DateTime.Now
            };
            var exception = new Exception("Test exception");

            _apprenticeFeedbackApi.Setup(api => api.TrackFeedbackTransactionClick(
                It.IsAny<long>(),
                It.IsAny<FeedbackTransactionClickRequest>()))
                .Throws(exception);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(message, _messageHandlerContext));
            Assert.That(ex.Message, Is.EqualTo("Test exception"));
            _logger.VerifyLogging(LogLevel.Error, Times.Once());
        }
    }
}
