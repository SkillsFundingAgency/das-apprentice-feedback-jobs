
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System;
using System.Threading.Tasks;


namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateEmailTransactionsHttpTrigger
    {
        private readonly ILogger<GenerateEmailTransactionsHttpTrigger> _log;
        private readonly IApprenticeFeedbackApi _api;

        public GenerateEmailTransactionsHttpTrigger(ILogger<GenerateEmailTransactionsHttpTrigger> log, IApprenticeFeedbackApi api)
        {
            _api = api;
            _log = log;
        }

        [FunctionName(nameof(GenerateEmailTransactionsHttpTrigger))]
        public async Task Run([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest request, ILogger log)
        {
            try
            {
                _log.LogInformation("Starting GenerateEmailTransactions");
                GetEmailTransactionsResponse response = await _api.GenerateEmailTransactions();
                _log.LogInformation($"GenerateEmailTransactions completed with {response.Count} transactions with created date {response.CreatedOn}");
                return;
            }
            catch (Exception e)
            {
                log.LogError(e, "GenerateEmailTransactions has failed");
            }
        }
    }
}
