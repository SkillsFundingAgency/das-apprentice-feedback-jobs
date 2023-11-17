using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class UpdateApprenticeFeedbackTargetHttpTrigger
    {
        private readonly UpdateApprenticeFeedbackTargetTimerTrigger _apprenticeFeedbackTargetUpdateHttpTrigger;
        private readonly ILogger<UpdateApprenticeFeedbackTargetTimerTrigger> _log;

        public UpdateApprenticeFeedbackTargetHttpTrigger(
            UpdateApprenticeFeedbackTargetTimerTrigger apprenticeFeedbackTargetUpdateTrigger,
            ILogger<UpdateApprenticeFeedbackTargetTimerTrigger> log)
        {
            _apprenticeFeedbackTargetUpdateHttpTrigger = apprenticeFeedbackTargetUpdateTrigger;
            _log = log;
        }

#if DEBUG
        [FunctionName(nameof(UpdateApprenticeFeedbackTargetHttp))]
        public async Task<IActionResult> UpdateApprenticeFeedbackTargetHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestMessage request,
            [DurableClient] IDurableOrchestrationClient orchestrationClient
        )
        {
            return new OkObjectResult($"Orchestration instance id = {await _apprenticeFeedbackTargetUpdateHttpTrigger.RunOrchestrator(orchestrationClient)}");
        }
#endif
    }
}
