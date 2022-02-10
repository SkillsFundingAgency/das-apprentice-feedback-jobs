using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public static class GenerateFeedbackRequestEmails
    {
        [FunctionName("GenerateFeedbackRequestEmails")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
