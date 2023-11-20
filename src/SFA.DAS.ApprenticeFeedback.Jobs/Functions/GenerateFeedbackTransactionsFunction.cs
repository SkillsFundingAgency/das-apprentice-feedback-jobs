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
        private readonly IApprenticeFeedbackApi _api;

        public GenerateFeedbackTransactionsFunction(ILogger<GenerateFeedbackTransactionsFunction> log, IApprenticeFeedbackApi api)
        {
            _api = api;
            _log = log;
        }

        [FunctionName(nameof(GenerateFeedbackTransactionsTimer))]
        public async Task GenerateFeedbackTransactionsTimer(
            [TimerTrigger("%FunctionsOptions:GenerateFeedbackTransactionsSchedule%", RunOnStartup = false)] TimerInfo myTimer,
            ILogger log)
        {
            try
            {
                _log.LogInformation("Starting GenerateFeedbackTransactionsTimer");
                GetEmailTransactionsResponse response = await _api.GenerateEmailTransactions();
                _log.LogInformation($"GenerateFeedbackTransactionsTimer completed with {response.Count} transactions with created date {response.CreatedOn}");
                return;
            }
            catch (Exception e)
            {
                log.LogError(e, "GenerateFeedbackTransactionsTimer has failed");
            }
        }

#if DEBUG
        [FunctionName(nameof(GenerateFeedbackTransactionsHttp))]
        public async Task GenerateFeedbackTransactionsHttp([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request, 
            ILogger log)
        {
            try
            {
                _log.LogInformation("Starting GenerateFeedbackTransactionsHttp");
                GetEmailTransactionsResponse response = await _api.GenerateEmailTransactions();
                _log.LogInformation($"GenerateFeedbackTransactionsHttp completed with {response.Count} transactions with created date {response.CreatedOn}");
                return;
            }
            catch (Exception e)
            {
                log.LogError(e, "GenerateFeedbackTransactionsHttp has failed");
            }
        }
#endif
    }
}
