using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateFeedbackTransactionsFunction(
        ILogger<GenerateFeedbackTransactionsFunction> log,
        IApprenticeFeedbackApi apprenticeFeedbackApi)
    {
        [Function(nameof(GenerateFeedbackTransactionsTimer))]
        public async Task GenerateFeedbackTransactionsTimer(
            [TimerTrigger("%GenerateFeedbackTransactionsSchedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            try
            {
                log.LogInformation("GenerateFeedbackTransactionsTimer has started");
                await apprenticeFeedbackApi.GenerateEmailTransactions();
                log.LogInformation($"GenerateFeedbackTransactionsTimer has finished");
                return;
            }
            catch (Exception e)
            {
                log.LogError(e, "GenerateFeedbackTransactionsTimer has failed");
                throw;
            }
        }

#if DEBUG
        [Function(nameof(GenerateFeedbackTransactionsHttp))]
        public async Task GenerateFeedbackTransactionsHttp([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request)
        {
            try
            {
                log.LogInformation("GenerateFeedbackTransactionsHttp has started");
                await apprenticeFeedbackApi.GenerateEmailTransactions();
                log.LogInformation($"GenerateFeedbackTransactionsHttp has finished");
                return;
            }
            catch (Exception e)
            {
                log.LogError(e, "GenerateFeedbackTransactionsHttp has failed");
                throw;
            }
        }
#endif
    }
}
