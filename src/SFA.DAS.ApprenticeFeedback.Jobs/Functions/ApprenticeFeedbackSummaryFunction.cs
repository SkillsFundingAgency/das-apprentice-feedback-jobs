using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ApprenticeFeedbackSummaryFunction
    {
        private readonly ILogger<GenerateFeedbackTransactionsFunction> _log;
        private readonly IApprenticeFeedbackApi _api;

        public ApprenticeFeedbackSummaryFunction(ILogger<GenerateFeedbackTransactionsFunction> log, IApprenticeFeedbackApi api)
        {
            _log = log;
            _api = api;
        }

        [FunctionName("ApprenticeFeedbackSummaryTimer")]
        public async Task ApprenticeFeedbackSummaryTimer(
            [TimerTrigger("%FunctionsOptions:ApprenticeFeedbackSummarySchedule%")] TimerInfo timer)
        {
            try
            {
                _log.LogInformation($"ApprenticeFeedbackSummaryTimer has started");
                await _api.GenerateFeedbackSummaries();
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
        public async Task ApprenticeFeedbackSummaryHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request)
        {
            try
            {
                _log.LogInformation($"ApprenticeFeedbackSummaryHttp has started");
                await _api.GenerateFeedbackSummaries();
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
