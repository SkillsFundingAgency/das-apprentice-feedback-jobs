using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateFeedbackTransactionsTimerTrigger
    {
        private readonly ILogger<GenerateFeedbackTransactionsTimerTrigger> _log;
        private readonly IApprenticeFeedbackApi _api;

        public GenerateFeedbackTransactionsTimerTrigger(ILogger<GenerateFeedbackTransactionsTimerTrigger> log, IApprenticeFeedbackApi api)
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
    }
}
