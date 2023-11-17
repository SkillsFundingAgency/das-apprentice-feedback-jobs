using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateFeedbackTransactionsHttpTrigger
    {
        private readonly ILogger<GenerateFeedbackTransactionsHttpTrigger> _log;
        private readonly IApprenticeFeedbackApi _api;

        public GenerateFeedbackTransactionsHttpTrigger(ILogger<GenerateFeedbackTransactionsHttpTrigger> log, IApprenticeFeedbackApi api)
        {
            _api = api;
            _log = log;
        }
#if DEBUG
        [FunctionName(nameof(GenerateFeedbackTransactionsHttp))]
        public async Task GenerateFeedbackTransactionsHttp([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request, ILogger log)
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
