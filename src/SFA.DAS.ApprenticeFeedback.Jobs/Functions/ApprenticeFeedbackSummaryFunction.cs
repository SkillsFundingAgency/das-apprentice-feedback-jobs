using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Messages.Commands;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ApprenticeFeedbackSummaryFunction
    {
        private readonly IFunctionEndpoint _endpoint;

        public ApprenticeFeedbackSummaryFunction(IFunctionEndpoint endpoint) => _endpoint = endpoint;

        [FunctionName("ApprenticeFeedbackSummaryTimer")]
        public void ApprenticeFeedbackSummaryTimer(
            [TimerTrigger("%FunctionsOptions:ApprenticeFeedbackSummarySchedule%")] TimerInfo timer, 
            ExecutionContext executionContext, 
            ILogger logger)
        {
            logger.LogInformation($"Starting ApprenticeFeedbackSummaryTimer");
            var sendOptions = SendLocally.Options;
            _endpoint.Send(new GenerateApprenticeFeedbackSummariesCommand(), sendOptions, executionContext, logger);
        }

#if DEBUG
        [FunctionName("ApprenticeFeedbackSummaryHttp")]
        public void ApprenticeFeedbackSummaryHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("Starting ApprenticeFeedbackSummaryHttp");
            var sendOptions = SendLocally.Options;
            _endpoint.Send(new GenerateApprenticeFeedbackSummariesCommand(), sendOptions, executionContext, logger);
        }
#endif
    }
}
