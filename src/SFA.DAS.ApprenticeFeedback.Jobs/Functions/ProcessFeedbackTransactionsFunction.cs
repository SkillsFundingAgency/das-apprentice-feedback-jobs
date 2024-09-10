using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
using Microsoft.Azure.Functions.Worker;
using ExecutionContext = System.Threading.ExecutionContext;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ProcessFeedbackTransactionsFunction(
        ApplicationConfiguration appConfig,
        ILogger<ProcessFeedbackTransactionsFunction> log,
        IApprenticeFeedbackApi apprenticeFeedbackApi)
    {
        [Function(nameof(ProcessFeedbackTransactionsActivity))]
        public async Task<SendApprenticeFeedbackEmailResponse> ProcessFeedbackTransactionsActivity(
            [ActivityTrigger] FeedbackTransaction emailTarget)
        {
            log.LogInformation($"Activity function is performing email send activity for apprentice feedback transaction Id {emailTarget.FeedbackTransactionId}");

            var response = await apprenticeFeedbackApi.ProcessEmailTransaction(emailTarget.FeedbackTransactionId, emailTarget);

            log.LogInformation($"Activity function response: apprentice feedback transaction Id {response.FeedbackTransactionId} email status = {response.EmailStatus}");

            return response;
        }

        [Function(nameof(ProcessFeedbackTransactionsOrchestrator))]
        public async Task<SendApprenticeFeedbackEmailResponse[]> ProcessFeedbackTransactionsOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext orchestrationContext,
            ExecutionContext executionContext)
        {
            if (orchestrationContext.IsReplaying)
            {
                log.LogInformation($"Orchestrator function is replaying");
            }

            var emailTargets = orchestrationContext.GetInput<IEnumerable<FeedbackTransaction>>();
            var tasks = emailTargets
                .Select(et => orchestrationContext.CallActivityAsync<SendApprenticeFeedbackEmailResponse>(
                        nameof(ProcessFeedbackTransactionsActivity),
                        et
                )
            );

            var responses = await Task.WhenAll(tasks);

            log.LogInformation($"Orchestrator function finished");

            return responses;
        }

        [Function(nameof(ProcessFeedbackTransactionsTimer))]
        public async Task ProcessFeedbackTransactionsTimer(
            [TimerTrigger("%ProcessFeedbackTransactionsSchedule%")] TimerInfo myTimer,
            [DurableClient] DurableTaskClient orchestrationClient)
        {
            log.LogInformation($"Starting ProcessFeedbackTransactionsTimer, Orchestration instance id = {await RunOrchestrator(orchestrationClient)}");
        }

#if DEBUG
        [Function(nameof(ProcessFeedbackTransactionsHttp))]
        public async Task<IActionResult> ProcessFeedbackTransactionsHttp(
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
                var emailTargets = await apprenticeFeedbackApi.GetFeedbackTransactionsToEmail(appConfig.EmailBatchSize);

                var result = await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(
                    nameof(ProcessFeedbackTransactionsOrchestrator),
                    emailTargets
                );

                return result;
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Orchestrator failed.");
                throw;
            }
        }
    }
}
