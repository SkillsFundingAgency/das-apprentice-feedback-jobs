using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ProcessFeedbackTargetVariantsFunction
    {
        private readonly ILogger<ProcessFeedbackTargetVariantsFunction> _logger;
        private readonly IFeedbackTargetVariantBlobProcessor _blobProcessor;

        public ProcessFeedbackTargetVariantsFunction(
            ILogger<ProcessFeedbackTargetVariantsFunction> log,
            IFeedbackTargetVariantBlobProcessor blobProcessor)
        {
            _logger = log;
            _blobProcessor = blobProcessor;
        }

        [Function(nameof(ProcessFeedbackTargetVariantsTimer))]
        public async Task ProcessFeedbackTargetVariantsTimer([TimerTrigger("%FunctionsOptions:ProcessFeedbackTargetVariantsSchedule%")] TimerInfo timer, ILogger logger)
        {
            _logger.LogInformation($"ProcessFeedbackTargetVariantsFunction executed at: {DateTime.Now}");
            await Run(nameof(ProcessFeedbackTargetVariantsTimer));
            _logger.LogInformation($"ProcessFeedbackTargetVariantsFunction completed at: {DateTime.Now}");
        }

#if DEBUG
        [Function(nameof(ProcessFeedbackTargetVariantsHttp))]
        public async Task ProcessFeedbackTargetVariantsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation($"ProcessFeedbackTargetVariantsHttp executed at: {DateTime.Now}");

            await Run(nameof(ProcessFeedbackTargetVariantsHttp));

            _logger.LogInformation($"ProcessFeedbackTargetVariantsHttp completed at: {DateTime.Now}");

            // Return a success response
            req.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await req.HttpContext.Response.WriteAsync("Processing completed successfully.");
        }
#endif

        private async Task Run(string functionName)
        {
            try
            {
                _logger.LogInformation("{FunctionName} has started", functionName);
                await _blobProcessor.ProcessBlobs();
                _logger.LogInformation("{FunctionName} has finished", functionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{FunctionName} has failed", functionName);
                throw;
            }
        }
    }

}