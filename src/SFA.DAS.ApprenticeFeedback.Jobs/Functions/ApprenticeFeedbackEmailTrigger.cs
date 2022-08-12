using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ApprenticeFeedbackEmailTrigger
    {
        private readonly ILogger<ApprenticeFeedbackEmailTrigger> _log;
        private readonly ApplicationConfiguration _appConfig;
        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;

        public ApprenticeFeedbackEmailTrigger(
            ApplicationConfiguration appConfig
            , ILogger<ApprenticeFeedbackEmailTrigger> log
            , IApprenticeFeedbackApi apprenticeFeedbackApi
            )
        {
            _appConfig = appConfig;
            _log = log;
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
        }

        // Activity function does the work. Called by the orchestrator.
        // Recommendation is that this activity should never run for longer than 5 minutes.
        [FunctionName(nameof(ApprenticeFeedbackEmailActivity))]
        public async Task<SendApprenticeFeedbackEmailResponse> ApprenticeFeedbackEmailActivity(
            [ActivityTrigger] ApprenticeFeedbackTransaction emailTarget)
        {
            _log.LogInformation($"Activity function is performing email send activity for apprentice feedback transaction Id {emailTarget.ApprenticeFeedbackTransactionId}");
            
            var response = await _apprenticeFeedbackApi.ProcessEmailTransaction(emailTarget.ApprenticeFeedbackTransactionId, emailTarget);

            _log.LogInformation($"Activity function response: apprentice feedback transaction Id {response.ApprenticeFeedbackTransactionId} email status = {response.EmailStatus}");

            return response;
        }


        // Orchestrator function kicks-off the activity functions and gathers the responses
        [FunctionName(nameof(ApprenticeFeedbackEmailOrchestrator))]
        public async Task<SendApprenticeFeedbackEmailResponse[]> ApprenticeFeedbackEmailOrchestrator(
           [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext
            , ExecutionContext executionContext
           )
        {
            if (orchestrationContext.IsReplaying)
            {
                _log.LogInformation($"Orchestrator function is replaying");
            }

            var emailTargets = orchestrationContext.GetInput<IEnumerable<ApprenticeFeedbackTransaction>>();
            var tasks = emailTargets
                .Select(et => orchestrationContext.CallActivityAsync<SendApprenticeFeedbackEmailResponse>(
                        nameof(ApprenticeFeedbackEmailActivity),
                        et
                )
            );

            var responses = await Task.WhenAll(tasks);

            _log.LogInformation($"Orchestrator function finished");

            return responses;
        }

        // Timer entry point - trigger the orchestration
        [FunctionName(nameof(ApprenticeFeedbackEmailTimerTrigger))]
        public async Task ApprenticeFeedbackEmailTimerTrigger(
           [TimerTrigger("%FunctionsOptions:ApprenticeFeedbackEmailSchedule%"
#if (RUNONSTARTUP)
            , RunOnStartup=true  
#endif
            )] TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient orchestrationClient
        )
        {
            _log.LogInformation("Timer fired.");
            await RunOrchestrator(orchestrationClient);
        }

        // Http entry point - manually trigger the orchestration
        [FunctionName(nameof(ApprenticeFeedbackEmailHttpTrigger))]
        public async Task<IActionResult> ApprenticeFeedbackEmailHttpTrigger(
            [HttpTrigger(AuthorizationLevel.Function, "PUT")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient orchestrationClient
        )
        {
            _log.LogInformation("Manual http trigger fired.");
            return new OkObjectResult($"Orchestration instance id = {await RunOrchestrator(orchestrationClient)}");
        }

        private async Task<string> RunOrchestrator(IDurableOrchestrationClient orchestrationClient)
        {
            try
            {
                // Get a batch of email targets.
                var emailTargets = await _apprenticeFeedbackApi.GetFeedbackTransactionsToEmail(_appConfig.EmailBatchSize);

                // Start the orchestration.
                var result = await orchestrationClient.StartNewAsync(
                    nameof(ApprenticeFeedbackEmailOrchestrator),
                    emailTargets
                );

                return result;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Orchestrator failed.");
                throw;
            }
        }
    }
}
