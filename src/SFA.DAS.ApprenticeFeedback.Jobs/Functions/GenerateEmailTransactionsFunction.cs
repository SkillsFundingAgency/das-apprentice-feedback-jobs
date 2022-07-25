
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;


namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateEmailTransactionsFunction
    {
        private readonly ILogger<GenerateEmailTransactionsFunction> _log;
        private readonly IApprenticeFeedbackEmailTransactionApi _api;

        public GenerateEmailTransactionsFunction(ILogger<GenerateEmailTransactionsFunction> log, IApprenticeFeedbackEmailTransactionApi api) //, IApprenticeFeedbackEmailTransactionService service)
        {
            _api = api;
            _log = log;
        }

        [FunctionName("GenerateEmailTransactions")]
        public async Task/*<IActionResult>*/ Run([TimerTrigger("%FunctionsOptions:GenerateEmailTransactionsOptions:Schedule%", RunOnStartup = true)] TimerInfo myTimer,
            ILogger log)
        {
            try
            {
                _log.LogInformation("Starting GenerateEmailTransactions");
                GetEmailTransactionsResponse response = await _api.GenerateEmailTransactions(new GenerateEmailTransactionsRequest());
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
