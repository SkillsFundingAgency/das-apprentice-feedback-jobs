using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers
{
    public class ApprenticeshipConfirmationConfirmedEventHandler : IHandleMessages<ApprenticeshipConfirmationConfirmedEvent>
    {
        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;
        private readonly ILogger<ApprenticeshipConfirmationConfirmedEventHandler> _logger;

        public ApprenticeshipConfirmationConfirmedEventHandler(
            IApprenticeFeedbackApi apprenticeFeedbackApi,
            ILogger<ApprenticeshipConfirmationConfirmedEventHandler> logger)
        {
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipConfirmationConfirmedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handling ApprenticeshipConfirmationConfirmedEvent for {ApprenticeshipId} (Commitments Apprentice Id {CommitmentsApprenticeshipId})"
                , message.ApprenticeshipId, message.CommitmentsApprenticeshipId);

            await _apprenticeFeedbackApi.CreateFeedbackTarget((ApprenticeConfirmedDetails)message);
        }
    }
}
