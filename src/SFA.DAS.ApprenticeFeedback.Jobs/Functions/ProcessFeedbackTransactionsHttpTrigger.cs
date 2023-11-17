using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ProcessFeedbackTransactionsHttpTrigger
    {
        private readonly ProcessFeedbackTransactionsTimerTrigger _processFeedbackTransactionsTimerTrigger;

        public ProcessFeedbackTransactionsHttpTrigger(
            ProcessFeedbackTransactionsTimerTrigger processFeedbackTransactionsTimerTrigger,
            ILogger<UpdateApprenticeFeedbackTargetTimerTrigger> log)
        {
            _processFeedbackTransactionsTimerTrigger = processFeedbackTransactionsTimerTrigger;
        }

#if DEBUG
        [FunctionName(nameof(ProcessFeedbackTransactionsHttp))]
        public async Task<IActionResult> ProcessFeedbackTransactionsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestMessage request,
            [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            return new OkObjectResult($"Orchestration instance id = {await _processFeedbackTransactionsTimerTrigger.RunOrchestrator(orchestrationClient)}");
        }
#endif
    }
}
