using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers
{
    // @ToDo: To be completed, pending conversation/fully-formed JIRA for retry logic

    public class ApprenticeFeedbackTargetUpdateFailedHandler : IHandleMessages<ApprenticeFeedbackTargetUpdateResponse>
    {
        //private readonly IApprenticeFeedbackApi _api;
        private readonly ILogger<ApprenticeFeedbackTargetUpdateFailedHandler> _logger;

        public ApprenticeFeedbackTargetUpdateFailedHandler(
            //IApprenticeFeedbackApi api,
            ILogger<ApprenticeFeedbackTargetUpdateFailedHandler> logger)
        {
            //_api = api;
            _logger = logger;
        }

        public async Task Handle(ApprenticeFeedbackTargetUpdateResponse message, IMessageHandlerContext context)
        {
            /*
            _logger.LogInformation("Handling ApprenticeshipConfirmationConfirmedEvent for {ApprenticeshipId} (Commitments Apprentice Id {CommitmentsApprenticeshipId})"
                , message.ApprenticeshipId, message.CommitmentsApprenticeshipId);

            await _api.CreateFeedbackTarget((ApprenticeConfirmedDetails)message);
            */
        }
    }
}
