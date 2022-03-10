using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers
{
    public class ApprenticeshipConfirmationCommencedEventHandler : IHandleMessages<ApprenticeshipConfirmationConfirmedEvent>
    {
        private readonly ILogger<ApprenticeshipConfirmationCommencedEventHandler> _logger;

        public ApprenticeshipConfirmationCommencedEventHandler(ILogger<ApprenticeshipConfirmationCommencedEventHandler> logger)
        {
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipConfirmationConfirmedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handling ApprenticeshipConfirmationConfirmedEvent for {ApprenticeshipId} (Commitments Apprentice Id {CommitmentsApprenticeshipId})", message.ApprenticeshipId, message.CommitmentsApprenticeshipId);
            await Task.CompletedTask;
        }
    }
}
