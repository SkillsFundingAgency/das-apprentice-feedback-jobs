using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ProcessFeedbackTransactionsTimerTrigger
    {
        private readonly ApplicationConfiguration _appConfig;
        private readonly ILogger<ProcessFeedbackTransactionsTimerTrigger> _log;
        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;

        public ProcessFeedbackTransactionsTimerTrigger(
            ApplicationConfiguration appConfig, 
            ILogger<ProcessFeedbackTransactionsTimerTrigger> log,
            IApprenticeFeedbackApi apprenticeFeedbackApi
            )
        {
            _appConfig = appConfig;
            _log = log;
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
        }

        [FunctionName(nameof(ProcessFeedbackTransactionsActivity))]
        public async Task<SendApprenticeFeedbackEmailResponse> ProcessFeedbackTransactionsActivity(
            [ActivityTrigger] FeedbackTransaction emailTarget)
        {
            _log.LogInformation($"Activity function is performing email send activity for apprentice feedback transaction Id {emailTarget.FeedbackTransactionId}");

            var response = await _apprenticeFeedbackApi.ProcessEmailTransaction(emailTarget.FeedbackTransactionId, emailTarget);

            _log.LogInformation($"Activity function response: apprentice feedback transaction Id {response.FeedbackTransactionId} email status = {response.EmailStatus}");

            return response;
        }

        [FunctionName(nameof(ProcessFeedbackTransactionsOrchestrator))]
        public async Task<SendApprenticeFeedbackEmailResponse[]> ProcessFeedbackTransactionsOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ExecutionContext executionContext)
        {
            if (orchestrationContext.IsReplaying)
            {
                _log.LogInformation($"Orchestrator function is replaying");
            }

            var emailTargets = orchestrationContext.GetInput<IEnumerable<FeedbackTransaction>>();
            var tasks = emailTargets
                .Select(et => orchestrationContext.CallActivityAsync<SendApprenticeFeedbackEmailResponse>(
                        nameof(ProcessFeedbackTransactionsActivity),
                        et
                )
            );

            var responses = await Task.WhenAll(tasks);

            _log.LogInformation($"Orchestrator function finished");

            return responses;
        }

        [FunctionName(nameof(ProcessFeedbackTransactionsTimer))]
        public async Task ProcessFeedbackTransactionsTimer(
            [TimerTrigger("%FunctionsOptions:ProcessFeedbackTransactionsSchedule%")] TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            _log.LogInformation($"Starting ProcessFeedbackTransactionsTimer, Orchestration instance id = {await RunOrchestrator(orchestrationClient)}");
        }

        public async Task<string> RunOrchestrator(IDurableOrchestrationClient orchestrationClient)
        {
            try
            {
                var emailTargets = await _apprenticeFeedbackApi.GetFeedbackTransactionsToEmail(_appConfig.EmailBatchSize);

                var result = await orchestrationClient.StartNewAsync(
                    nameof(ProcessFeedbackTransactionsOrchestrator),
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
