using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using SFA.DAS.AssessorService.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Handlers.AssessorHandlers
{
    public class OutcomeRecordedEventHandler : IHandleMessages<OutcomeRecordedEvent>
    {
        public const string TemplateName = "ApprenticeshipOutcome";

        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;
        private readonly ILogger<OutcomeRecordedEventHandler> _logger;

        public OutcomeRecordedEventHandler(
            IApprenticeFeedbackApi apprenticeFeedbackApi,
            ILogger<OutcomeRecordedEventHandler> logger)
        {
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
            _logger = logger;
        }

        public async Task Handle(OutcomeRecordedEvent message, IMessageHandlerContext context)
        {
            try
            {
                var jsonMessage = JsonConvert.SerializeObject(message);
                _logger.LogInformation("Handling OutcomeRecordedEvent {JsonMessage}", jsonMessage);

                if (message.Outcome != OutcomeType.Pass)
                {
                    _logger.LogInformation("OutcomeRecordedEvent for apprenticeship {ApprenticeshipId} ignored as outcome is {Outcome}", message.ApprenticeshipId, message.Outcome);
                    return;
                }

                await _apprenticeFeedbackApi.CreateFeedbackTransaction(new CreateFeedbackTransactionRequest
                {
                    ApprenticeshipId = message.ApprenticeshipId,
                    TemplateName = TemplateName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to handle OutcomeRecordedEvent.");
                throw;
            }
        }
    }
}
