using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
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
    public class UpdateApprenticeFeedbackTargetFunction
    {
        private readonly IApprenticeFeedbackApi _apiClient;
        private readonly ApplicationConfiguration _appConfig;
        private readonly ILogger<UpdateApprenticeFeedbackTargetFunction> _log;

        public UpdateApprenticeFeedbackTargetFunction(
            IApprenticeFeedbackApi apiClient, 
            ApplicationConfiguration appConfig,
            ILogger<UpdateApprenticeFeedbackTargetFunction> log)
        {
            _apiClient = apiClient;
            _appConfig = appConfig;
            _log = log;
        }

        [FunctionName(nameof(UpdateApprenticeFeedbackTargetActivity))]
        public async Task<UpdateApprenticeFeedbackTargetResponse> UpdateApprenticeFeedbackTargetActivity(
            [ActivityTrigger] FeedbackTargetForUpdate apprenticeFeedbackTargetToUpdate)
        {
            _log.LogInformation($"Activity function is updating apprentice feedback target id '{apprenticeFeedbackTargetToUpdate.ApprenticeFeedbackTargetId}'...");
            
            var response = await _apiClient.UpdateFeedbackTarget(new UpdateApprenticeFeedbackTargetRequest()
            {
                ApprenticeFeedbackTargetId = apprenticeFeedbackTargetToUpdate.ApprenticeFeedbackTargetId,
                ApprenticeId = apprenticeFeedbackTargetToUpdate.ApprenticeId,
                ApprenticeshipId = apprenticeFeedbackTargetToUpdate.ApprenticeshipId
            });

            response.ApprenticeFeedbackTarget = apprenticeFeedbackTargetToUpdate;

            return response;
        }

        [FunctionName(nameof(UpdateApprenticeFeedbackTargetOrchestrator))]
        public async Task<UpdateApprenticeFeedbackTargetResponse[]> UpdateApprenticeFeedbackTargetOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ExecutionContext executionContext)
        {
            if (orchestrationContext.IsReplaying)
            {
                _log.LogInformation($"Orchestrator function is replaying");
            }

            var aftsForUpdate = orchestrationContext.GetInput<List<FeedbackTargetForUpdate>>();

            _log.LogInformation($"Orchestrator function has selected the following {aftsForUpdate.Count} apprentice feedback target(s) to update:");
            for (int i = 0; i < aftsForUpdate.Count; i++)
            {
                _log.LogTrace($"   [{i + 1}] - id={aftsForUpdate[i].ApprenticeFeedbackTargetId} apprenticeshipid={aftsForUpdate[i].ApprenticeshipId}");
            }

            var tasks = aftsForUpdate
                .Select(aft => orchestrationContext.CallActivityAsync<UpdateApprenticeFeedbackTargetResponse>(
                        nameof(UpdateApprenticeFeedbackTargetActivity),
                        aft
                )
            );

            var responses = await Task.WhenAll(tasks);

            _log.LogInformation($"Orchestrator function finished");

            return responses;
        }

        [FunctionName(nameof(UpdateApprenticeFeedbackTargetTimer))]
        public async Task UpdateApprenticeFeedbackTargetTimer(
            [TimerTrigger("%FunctionsOptions:UpdateApprenticeFeedbackTargetSchedule%")] TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            _log.LogInformation($"Starting UpdateApprenticeFeedbackTargetTimer, Orchestration instance id = {await RunOrchestrator(orchestrationClient)}");
        }

#if DEBUG
        [FunctionName(nameof(UpdateApprenticeFeedbackTargetHttp))]
        public async Task<IActionResult> UpdateApprenticeFeedbackTargetHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestMessage request,
            [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            return new OkObjectResult($"Orchestration instance id = {await RunOrchestrator(orchestrationClient)}");
        }
#endif

        private async Task<string> RunOrchestrator(IDurableOrchestrationClient orchestrationClient)
        {
            try
            {
                var feedbackTargetsForUpdate = await _apiClient.GetFeedbackTargetsForUpdate(_appConfig.UpdateBatchSize);

                var result = await orchestrationClient.StartNewAsync(
                    nameof(UpdateApprenticeFeedbackTargetOrchestrator),
                    feedbackTargetsForUpdate
                );

                return result;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Orchestrator failed. Cannot update feedback targets.");
                throw;
            }
        }
    }
}
