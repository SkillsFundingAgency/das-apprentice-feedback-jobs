using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Messages.Commands;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ApprenticeFeedbackSummaryTimerTrigger
    {
        private readonly IFunctionEndpoint _endpoint;

        public ApprenticeFeedbackSummaryTimerTrigger(IFunctionEndpoint endpoint) => _endpoint = endpoint;

        [FunctionName("ApprenticeFeedbackSummaryTimer")]
        public void ApprenticeFeedbackSummaryTimer(
            [TimerTrigger("%FunctionsOptions:ApprenticeFeedbackSummarySchedule%")] TimerInfo timer, 
            ExecutionContext executionContext, ILogger logger)
        {
            logger.LogInformation($"Starting ApprenticeFeedbackSummaryTimer");
            var sendOptions = SendLocally.Options;
            _endpoint.Send(new GenerateApprenticeFeedbackSummariesCommand(), sendOptions, executionContext, logger);
        }
    }
}
