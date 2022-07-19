using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Messages.Commands;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure;
using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ApprenticeFeedbackSummaryTimerTrigger
    {
        private readonly IFunctionEndpoint endpoint;

        public ApprenticeFeedbackSummaryTimerTrigger(IFunctionEndpoint endpoint) => this.endpoint = endpoint;

        [FunctionName("GenerateApprenticeFeedbackSummaries")]
        public void ApprenticeFeedbackSummary([TimerTrigger("0 */3 * * * *")] TimerInfo timer, ExecutionContext executionContext, ILogger logger)
        {
            logger.LogInformation($"GenerateApprenticeFeedbackSummaries Timer trigger function executed at {DateTime.UtcNow}");
            var sendOptions = SendLocally.Options;
            endpoint.Send(new GenerateApprenticeFeedbackSummariesCommand(), sendOptions, executionContext, logger);
        }
    }
}
