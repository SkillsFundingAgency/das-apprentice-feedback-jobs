using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Models;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class UpdateApprenticeFeedbackTargetFunction(
        IApprenticeFeedbackApi apprenticeFeedbackApi,
        ApplicationConfiguration appConfig,
        ILogger<UpdateApprenticeFeedbackTargetFunction> log)
    {
        [Function(nameof(UpdateApprenticeFeedbackTargetActivity))]
        public async Task<UpdateApprenticeFeedbackTargetResponse> UpdateApprenticeFeedbackTargetActivity(
            [ActivityTrigger] FeedbackTargetForUpdate apprenticeFeedbackTargetToUpdate)
        {
            log.LogInformation($"Activity function is updating apprentice feedback target id '{apprenticeFeedbackTargetToUpdate.ApprenticeFeedbackTargetId}'...");
            
            var response = await apprenticeFeedbackApi.UpdateFeedbackTarget(new UpdateApprenticeFeedbackTargetRequest()
            {
                ApprenticeFeedbackTargetId = apprenticeFeedbackTargetToUpdate.ApprenticeFeedbackTargetId,
                ApprenticeId = apprenticeFeedbackTargetToUpdate.ApprenticeId,
                ApprenticeshipId = apprenticeFeedbackTargetToUpdate.ApprenticeshipId
            });

            response.ApprenticeFeedbackTarget = apprenticeFeedbackTargetToUpdate;

            return response;
        }

        [Function(nameof(UpdateApprenticeFeedbackTargetOrchestrator))]
        public async Task<UpdateApprenticeFeedbackTargetResponse[]> UpdateApprenticeFeedbackTargetOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext orchestrationContext,
            List<FeedbackTargetForUpdate> aftsForUpdate)
        {
            log.LogInformation($"Orchestrator function has selected the following {aftsForUpdate.Count} apprentice feedback target(s) to update:");
            for (int i = 0; i < aftsForUpdate.Count; i++)
            {
                log.LogTrace($"   [{i + 1}] - id={aftsForUpdate[i].ApprenticeFeedbackTargetId} apprenticeshipid={aftsForUpdate[i].ApprenticeshipId}");
            }

            var tasks = aftsForUpdate
                .Select(aft => orchestrationContext.CallActivityAsync<UpdateApprenticeFeedbackTargetResponse>(
                        nameof(UpdateApprenticeFeedbackTargetActivity),
                        aft
                )
            );

            var responses = await Task.WhenAll(tasks);

            log.LogInformation($"Orchestrator function finished");

            return responses;
        }

        [Function(nameof(UpdateApprenticeFeedbackTargetTimer))]
        public async Task UpdateApprenticeFeedbackTargetTimer(
            [TimerTrigger("%UpdateApprenticeFeedbackTargetSchedule%")] TimerInfo myTimer,
            [DurableClient] DurableTaskClient orchestrationClient)
        {
            log.LogInformation($"Starting UpdateApprenticeFeedbackTargetTimer, Orchestration instance id = {await RunOrchestrator(orchestrationClient)}");
        }

#if DEBUG
        [Function(nameof(UpdateApprenticeFeedbackTargetHttp))]
        public async Task<IActionResult> UpdateApprenticeFeedbackTargetHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestMessage request,
            [DurableClient] DurableTaskClient orchestrationClient)
        {
            return new OkObjectResult($"Orchestration instance id = {await RunOrchestrator(orchestrationClient)}");
        }
#endif

        private async Task<string> RunOrchestrator(DurableTaskClient orchestrationClient)
        {
            try
            {
                var feedbackTargetsForUpdate = await apprenticeFeedbackApi.GetFeedbackTargetsForUpdate(appConfig.UpdateBatchSize);

                var result = await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(
                    nameof(UpdateApprenticeFeedbackTargetOrchestrator),
                    feedbackTargetsForUpdate
                );

                return result;
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Orchestrator failed. Cannot update feedback targets.");
                throw;
            }
        }
    }
}
