using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Messages.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers
{
    public class GenerateApprenticeFeedbackSummariesCommandHandler : IHandleMessages<GenerateApprenticeFeedbackSummariesCommand>
    {
        private readonly IApprenticeFeedbackApi _api;
        private readonly ILogger<GenerateApprenticeFeedbackSummariesCommandHandler> _logger;

        public GenerateApprenticeFeedbackSummariesCommandHandler(
            IApprenticeFeedbackApi api,
            ILogger<GenerateApprenticeFeedbackSummariesCommandHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task Handle(GenerateApprenticeFeedbackSummariesCommand message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Handling GenerateApprenticeFeedbackSummariesCommandHandler");
            await _api.GenerateFeedbackSummaries();
            _logger.LogInformation("Finished Handling GenerateApprenticeFeedbackSummariesCommandHandler");

        }
    }
}
