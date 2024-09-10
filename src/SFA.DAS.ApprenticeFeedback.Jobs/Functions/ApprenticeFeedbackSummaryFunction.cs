using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ApprenticeFeedbackSummaryFunction(
        ILogger<GenerateFeedbackTransactionsFunction> log,
        IApprenticeFeedbackApi apprenticeFeedbackApi)
    {
        [Function(nameof(ApprenticeFeedbackSummaryTimer))]
        public async Task ApprenticeFeedbackSummaryTimer([TimerTrigger("* */5 * * * *")] TimerInfo timer)
        {
            try
            {
                log.LogInformation($"ApprenticeFeedbackSummaryTimer has started");
                await apprenticeFeedbackApi.GenerateFeedbackSummaries();
                log.LogInformation($"ApprenticeFeedbackSummaryTimer has finished");
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"ApprenticeFeedbackSummaryTimer has failed");
                throw;
            }
        }

#if DEBUG
        [Function(nameof(ApprenticeFeedbackSummaryHttp))]
        public async Task ApprenticeFeedbackSummaryHttp([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request)
        {
            try
            {
                log.LogInformation($"ApprenticeFeedbackSummaryHttp has started");
                await apprenticeFeedbackApi.GenerateFeedbackSummaries();
                log.LogInformation($"ApprenticeFeedbackSummaryHttp has finished");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"ApprenticeFeedbackSummaryHttp has failed");
                throw;
            }
        }
#endif
    }
}
