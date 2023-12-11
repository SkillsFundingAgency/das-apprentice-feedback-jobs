using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Messages.Commands;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure;
using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ApprenticeFeedbackSummaryFunction
    {
        private readonly ILogger<GenerateFeedbackTransactionsFunction> _log;
        private readonly IFunctionEndpoint _endpoint;

        public ApprenticeFeedbackSummaryFunction(ILogger<GenerateFeedbackTransactionsFunction> log, IFunctionEndpoint endpoint)
        {
            _log = log;
            _endpoint = endpoint;
        }

        [FunctionName("ApprenticeFeedbackSummaryTimer")]
        public void ApprenticeFeedbackSummaryTimer(
            [TimerTrigger("%FunctionsOptions:ApprenticeFeedbackSummarySchedule%")] TimerInfo timer, 
            ExecutionContext executionContext, 
            ILogger logger)
        {
            try
            {
                _log.LogInformation($"ApprenticeFeedbackSummaryTimer has started");
                var sendOptions = SendLocally.Options;
                _endpoint.Send(new GenerateApprenticeFeedbackSummariesCommand(), sendOptions, executionContext, logger);
                _log.LogInformation($"ApprenticeFeedbackSummaryTimer has finished");
            }
            catch(Exception ex)
            {
                _log.LogError(ex, $"ApprenticeFeedbackSummaryTimer has failed");
                throw;
            }
        }

#if DEBUG
        [FunctionName("ApprenticeFeedbackSummaryHttp")]
        public void ApprenticeFeedbackSummaryHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request,
            ExecutionContext executionContext,
            ILogger logger)
        {
            try
            {
                _log.LogInformation($"ApprenticeFeedbackSummaryHttp has started");
                var sendOptions = SendLocally.Options;
                _endpoint.Send(new GenerateApprenticeFeedbackSummariesCommand(), sendOptions, executionContext, logger);
                _log.LogInformation($"ApprenticeFeedbackSummaryHttp has finished");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"ApprenticeFeedbackSummaryHttp has failed");
                throw;
            }
        }
#endif
    }
}
