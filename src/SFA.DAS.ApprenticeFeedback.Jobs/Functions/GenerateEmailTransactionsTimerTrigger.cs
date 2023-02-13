
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System;
using System.Threading.Tasks;


namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateEmailTransactionsTimerTrigger
    {
        private readonly ILogger<GenerateEmailTransactionsTimerTrigger> _log;
        private readonly IApprenticeFeedbackApi _api;

        public GenerateEmailTransactionsTimerTrigger(ILogger<GenerateEmailTransactionsTimerTrigger> log, IApprenticeFeedbackApi api)
        {
            _api = api;
            _log = log;
        }

        [FunctionName(nameof(GenerateEmailTransactionsTimerTrigger))]
        public async Task Run([TimerTrigger("%FunctionsOptions:GenerateEmailTransactionsSchedule%", RunOnStartup = false)] TimerInfo myTimer,
            ILogger log)
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
