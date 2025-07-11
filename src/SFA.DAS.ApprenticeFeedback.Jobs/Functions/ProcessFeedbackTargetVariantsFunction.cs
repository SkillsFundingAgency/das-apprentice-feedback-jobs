using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;
using Microsoft.Azure.Functions.Worker;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ProcessFeedbackTargetVariantsFunction(
        ILogger<ProcessFeedbackTargetVariantsFunction> log,
        IFeedbackTargetVariantBlobProcessor blobProcessor)
    {
        [Function(nameof(ProcessFeedbackTargetVariantsTimer))]
        public async Task ProcessFeedbackTargetVariantsTimer([TimerTrigger("%ProcessFeedbackTargetVariantsSchedule%")] TimerInfo timer, ILogger logger)
        {
            await Run(nameof(ProcessFeedbackTargetVariantsTimer));
        }

#if DEBUG
        [Function(nameof(ProcessFeedbackTargetVariantsHttp))]
        public async Task ProcessFeedbackTargetVariantsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            await Run(nameof(ProcessFeedbackTargetVariantsHttp));

            // Return a success response
            req.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await req.HttpContext.Response.WriteAsync("Processing completed successfully.");
        }
#endif

        private async Task Run(string functionName)
        {
            try
            {
                log.LogInformation("{FunctionName} has started", functionName);
                await blobProcessor.ProcessBlobs();
                log.LogInformation("{FunctionName} has finished", functionName);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "{FunctionName} has failed", functionName);
                throw;
            }
        }
    }
}