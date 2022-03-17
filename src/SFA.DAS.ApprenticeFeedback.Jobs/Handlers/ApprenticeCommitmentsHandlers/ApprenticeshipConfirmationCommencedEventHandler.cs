using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers
{
    public class ApprenticeshipConfirmationCommencedEventHandler : IHandleMessages<ApprenticeshipConfirmationConfirmedEvent>
    {
        private readonly IApprenticeFeedbackApi _api;
        private readonly ILogger<ApprenticeshipConfirmationCommencedEventHandler> _logger;

        public ApprenticeshipConfirmationCommencedEventHandler(
            IApprenticeFeedbackApi api,
            ILogger<ApprenticeshipConfirmationCommencedEventHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipConfirmationConfirmedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handling ApprenticeshipConfirmationConfirmedEvent for {ApprenticeshipId} (Commitments Apprentice Id {CommitmentsApprenticeshipId})"
                , message.ApprenticeshipId, message.CommitmentsApprenticeshipId);

            await _api.CreateFeedbackTarget((ApprenticeConfirmedDetails)message);
        }
    }
}
