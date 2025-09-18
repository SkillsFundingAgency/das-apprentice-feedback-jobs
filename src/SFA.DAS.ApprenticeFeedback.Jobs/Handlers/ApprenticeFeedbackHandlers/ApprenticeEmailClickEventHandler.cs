using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using SFA.DAS.ApprenticeFeedback.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeFeedbackHandlers
{
    public class ApprenticeEmailClickEventHandler : IHandleMessages<ApprenticeEmailClickEvent>
    {
        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;
        private readonly ILogger<ApprenticeEmailClickEventHandler> _logger;

        public ApprenticeEmailClickEventHandler(
            IApprenticeFeedbackApi apprenticeFeedbackApi,
            ILogger<ApprenticeEmailClickEventHandler> logger)
        {
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
            _logger = logger;
        }

        public async Task Handle(ApprenticeEmailClickEvent message, IMessageHandlerContext context)
        {
            try
            {
                var jsonMessage = JsonConvert.SerializeObject(message);
                _logger.LogInformation("Handling ApprenticeEmailClickEventHandler {JsonMessage}", jsonMessage);

                await _apprenticeFeedbackApi.TrackFeedbackTransactionClick(message.FeedbackTransactionId, new FeedbackTransactionClickRequest
                {
                    FeedbackTransactionId = message.FeedbackTransactionId,
                    ApprenticeFeedbackTargetId = message.ApprenticeFeedbackTargetId,
                    LinkName = message.Linkname,
                    LinkUrl = message.Link,
                    ClickedOn = message.ClickedOn
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Unable to handle ApprenticeEmailClickEvent.");
                throw;
            }
        }
    }
}
