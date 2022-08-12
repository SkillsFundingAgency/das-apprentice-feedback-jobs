using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Models;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    // Fan-out Durable function example :
    // https://www.bekk.christmas/post/2019/23/fan-outfan-in-for-high-scalability-with-durable-functions

    public class ApprenticeFeedbackTargetUpdateTrigger
    {
        private readonly IApprenticeFeedbackApi _apiClient;
        private readonly IFunctionEndpoint _endpoint;
        private readonly ILogger<ApprenticeFeedbackTargetUpdateTrigger> _log;
        private readonly ApplicationConfiguration _appConfig;

        public ApprenticeFeedbackTargetUpdateTrigger(
            IApprenticeFeedbackApi apiClient
            , IFunctionEndpoint endpoint
            , ApplicationConfiguration appConfig
            , ILogger<ApprenticeFeedbackTargetUpdateTrigger> log
            )
        {
            _apiClient = apiClient;
            _endpoint = endpoint;
            _appConfig = appConfig;
            // Use a logger injected into the constructor rather than a logger as an argument
            // to each function due to a bug in the framework where the log level is not controllable
            _log = log;  
        }


        // Activity function calls the outer API to perform the update of the Apprentice Feedback Target
        // Recommendation is that this activity should never run for longer than 5 minutes
        [FunctionName(nameof(UpdateApprenticeFeedbackTargetActivity))]
        public async Task<ApprenticeFeedbackTargetUpdateResponse> UpdateApprenticeFeedbackTargetActivity (
            [ActivityTrigger] FeedbackTargetForUpdate apprenticeFeedbackTargetToUpdate)
        {
            _log.LogInformation($"Activity function is updating apprentice feedback target id '{apprenticeFeedbackTargetToUpdate.ApprenticeFeedbackTargetId}'...");
            var response = await _apiClient.UpdateFeedbackTarget(new ApprenticeFeedbackTargetUpdateRequest() 
            { 
                ApprenticeFeedbackTargetId = apprenticeFeedbackTargetToUpdate.ApprenticeFeedbackTargetId,
                ApprenticeshipId = apprenticeFeedbackTargetToUpdate.ApprenticeshipId
            });
            response.ApprenticeFeedbackTarget = apprenticeFeedbackTargetToUpdate;

            return response;
        }


        // Orchestrator function kicks-off the activity functions and gathers the responses
        [FunctionName(nameof(UpdateApprenticeFeedbackOrchestrator))]
        public async Task<ApprenticeFeedbackTargetUpdateResponse[]> UpdateApprenticeFeedbackOrchestrator (
           [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext
            , ExecutionContext executionContext
           )
        {
            if(orchestrationContext.IsReplaying)
            {
                _log.LogInformation($"Orchestrator function is replaying");
            }

            var aftsForUpdate = orchestrationContext.GetInput<List<FeedbackTargetForUpdate>>();

            _log.LogInformation($"Orchestrator function has selected the following {aftsForUpdate.Count} apprentice feedback target(s) to update:");
            for(int i = 0; i < aftsForUpdate.Count; i++)
            {
                _log.LogInformation($"   [{i+1}] - id={aftsForUpdate[i].ApprenticeFeedbackTargetId} apprenticeshipid={aftsForUpdate[i].ApprenticeshipId}");
            }

            var tasks = aftsForUpdate
                .Select(aft => orchestrationContext.CallActivityAsync<ApprenticeFeedbackTargetUpdateResponse>(
                        nameof(UpdateApprenticeFeedbackTargetActivity),
                        aft
                )
            );

            var responses = await Task.WhenAll(tasks);

            //  TBD - "Stage "1.15" - retry logic for update failures

            _log.LogInformation($"Orchestrator function finished");

            return responses;
        }

        // Timer entry point - the orchestration client which fetches the apprentice feedback targets
        // to be updated and triggers the orchestration
        [FunctionName(nameof(UpdateApprenticeFeedbackTargetTimerTrigger))]
        public async Task UpdateApprenticeFeedbackTargetTimerTrigger(
           [TimerTrigger("%ApprenticeFeedbackTargetUpdateSchedule%"
#if DEBUG
            , RunOnStartup=true  // does not seem to work
#endif
            )] TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient orchestrationClient
        )
        {
            _log.LogInformation("Timer fired.");
            await RunOrchestrator(orchestrationClient);
        }

        [FunctionName(nameof(UpdateApprenticeFeedbackTargetManualTrigger))]
        public async Task<IActionResult> UpdateApprenticeFeedbackTargetManualTrigger(
            [HttpTrigger(AuthorizationLevel.Function, "GET")] HttpRequestMessage req,
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
                // Call outer API to get a batch of AFTs that require updating
                var feedbackTargetsForUpdate = await _apiClient.GetFeedbackTargetsForUpdate(_appConfig.UpdateBatchSize);

                // Start the orchestration
                var result = await orchestrationClient.StartNewAsync(
                    nameof(UpdateApprenticeFeedbackOrchestrator),
                    feedbackTargetsForUpdate
                );

                return result;
            }
            catch(Exception ex)
            {
                _log.LogCritical(ex, "Orchestrator failed. Cannot update feedback targets.");
                throw;
            }
        }

        [FunctionName(nameof(RetryQueueTrigger))]
        public async Task RetryQueueTrigger(
            [QueueTrigger("bundle-1")]
            CloudQueueMessage message,
            ILogger logger,
            ExecutionContext context)
        {
            await _endpoint.ProcessNonAtomic(null, context, logger);
        }
    }
}
