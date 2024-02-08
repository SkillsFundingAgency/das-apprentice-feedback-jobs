using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateFeedbackTransactionsFunction
    {
        private readonly ILogger<GenerateFeedbackTransactionsFunction> _log;
        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;

        public GenerateFeedbackTransactionsFunction(ILogger<GenerateFeedbackTransactionsFunction> log, IApprenticeFeedbackApi apprenticeFeedbackApi)
        {
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
            _log = log;
        }

        [FunctionName(nameof(GenerateFeedbackTransactionsTimer))]
        public async Task GenerateFeedbackTransactionsTimer(
            [TimerTrigger("%FunctionsOptions:GenerateFeedbackTransactionsSchedule%", RunOnStartup = false)] TimerInfo myTimer)
        {
            try
            {
                _log.LogInformation("GenerateFeedbackTransactionsTimer has started");
                await _apprenticeFeedbackApi.GenerateEmailTransactions();
                _log.LogInformation($"GenerateFeedbackTransactionsTimer has finished");
                return;
            }
            catch (Exception e)
            {
                _log.LogError(e, "GenerateFeedbackTransactionsTimer has failed");
                throw;
            }
        }

#if DEBUG
        [FunctionName(nameof(GenerateFeedbackTransactionsHttp))]
        public async Task GenerateFeedbackTransactionsHttp([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request)
        {
            try
            {
                _log.LogInformation("GenerateFeedbackTransactionsHttp has started");
                await _apprenticeFeedbackApi.GenerateEmailTransactions();
                _log.LogInformation($"GenerateFeedbackTransactionsHttp has finished");
                return;
            }
            catch (Exception e)
            {
                _log.LogError(e, "GenerateFeedbackTransactionsHttp has failed");
                throw;
            }
        }
#endif
    }
}
