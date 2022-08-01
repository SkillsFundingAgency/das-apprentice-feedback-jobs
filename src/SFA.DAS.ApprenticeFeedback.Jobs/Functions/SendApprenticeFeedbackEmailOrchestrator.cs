
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class SendApprenticeFeedbackEmailOrchestrator
    {
        private readonly ILogger<SendApprenticeFeedbackEmailOrchestrator> _logger;
        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;
        
        public SendApprenticeFeedbackEmailOrchestrator(IApprenticeFeedbackApi apprenticeFeedbackApi, ILogger<SendApprenticeFeedbackEmailOrchestrator> logger)
        {
            _logger = logger;
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
        }

        [FunctionName("SendApprenticeFeedbackEmailOrchestrator")]
        public async Task Run([TimerTrigger("%FunctionsOptions:SendApprenticeFeedbackEmailOrchestratorOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer, 
            IDurableOrchestrationContext context)
        {
            try
            {
                _logger.LogInformation("Starting Send Apprentice Feedback Email Orchestration");
                var batchSize = 100; // To come from configuration / appsettings
                var response = await _apprenticeFeedbackApi.GetFeedbackTransactionsToEmail(batchSize);
                _logger.LogInformation($"Received Email Batch with size of {response.Count()}");

                var paralellTasks = new List<Task>();

                foreach(var target in response)
                {
                    var activity = context.CallActivityAsync("ProcessApprenticeFeedbackEmailActivity", target);
                    paralellTasks.Add(activity);
                }

                await Task.WhenAll(paralellTasks);

                _logger.LogInformation($"Finished SendApprenticeFeedbackEmailOrchestrator Function");

            }
            catch (Exception e)
            {
                _logger.LogError(e, "SendApprenticeFeedbackEmailOrchestrator failed to complete.");
            }
        }
    }
}
