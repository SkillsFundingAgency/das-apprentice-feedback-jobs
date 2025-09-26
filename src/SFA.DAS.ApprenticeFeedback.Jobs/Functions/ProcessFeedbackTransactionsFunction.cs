using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Exceptions;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
using SFA.DAS.ApprenticeFeedback.Jobs.Services;

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
            log.LogDebug("Activity function is performing email send activity for apprentice feedback transaction Id {FeedbackTransactionId}", emailTarget.FeedbackTransactionId);

            var response = await apprenticeFeedbackApi.ProcessEmailTransaction(emailTarget.FeedbackTransactionId, emailTarget);

            log.LogDebug("Activity function response: apprentice feedback transaction Id {FeedbackTransactionId} email status = {EmailStatus}", response.FeedbackTransactionId, response.EmailStatus);

            return response;
        }

        [Function(nameof(ProcessFeedbackTransactionsOrchestrator))]
        public async Task<SendApprenticeFeedbackEmailResponse[]> ProcessFeedbackTransactionsOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext ctx)
        {
            var fanoutService = new WaveFanoutService(appConfig.EmailPerSecondCap);

            var results = await fanoutService.ExecuteAsync(
                ctx,
                ctx.GetInput<List<FeedbackTransaction>>() ?? [],
                (ctx, feedbackTransaction) => ctx.CallActivityAsync<SendApprenticeFeedbackEmailResponse>(
                    nameof(ProcessFeedbackTransactionsActivity), feedbackTransaction)
            );

            log.LogInformation($"ProcessFeedbackTransactions orchestrator function finished");

            return results.ToArray();
        }

        [Function(nameof(ProcessFeedbackTransactionsTimer))]
        public async Task ProcessFeedbackTransactionsTimer(
            [TimerTrigger("%ProcessFeedbackTransactionsSchedule%")] TimerInfo myTimer,
            [DurableClient] DurableTaskClient orchestrationClient)
        {
            log.LogInformation("ProcessFeedbackTransactions orchestrator function starting, orchestration instance id = {OrchestrationInstanceId}", await RunOrchestrator(orchestrationClient));
        }

#if DEBUG
        [Function(nameof(ProcessFeedbackTransactionsHttp))]
        public async Task<IActionResult> ProcessFeedbackTransactionsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestMessage request,
            [DurableClient] DurableTaskClient orchestrationClient)
        {
            return new OkObjectResult($"ProcessFeedbackTransactions orchestrator function starting, orchestration instance id = {await RunOrchestrator(orchestrationClient)}");
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
                throw new OrchestratorException("ProcessFeedbackTransactions orchestrator function failed.", ex);
            }
        }
    }
}
