using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers
{
    public class ApprenticeEmailClickEventHandler : IHandleMessages<ApprenticeEmailClickEvent>
    {
        private readonly IApprenticeFeedbackApi _api;
        private readonly ILogger<ApprenticeEmailClickEventHandler> _logger;

        public ApprenticeEmailClickEventHandler(
            IApprenticeFeedbackApi api,
            ILogger<ApprenticeEmailClickEventHandler> logger)
        {
            _api = api;
            _logger = logger;
        }

        public async Task Handle(ApprenticeEmailClickEvent message, IMessageHandlerContext context)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            _logger.LogInformation($"Handling ApprenticeEmailClickEventHandler {jsonMessage}");
        }
    }
}
