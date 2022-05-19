using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ApprenticeshipFeedbackSimulationHttpTrigger
    {
        private readonly IFunctionEndpoint endpoint;

        public ApprenticeshipFeedbackSimulationHttpTrigger(IFunctionEndpoint endpoint) => this.endpoint = endpoint;

        [FunctionName("HandleApprenticeshipConfirmedEventTrigger")]
        public Task<IActionResult> ApprenticeshipCreatedEvent(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "test-apprenticeship-confirmed-event")] HttpRequestMessage req,
            ExecutionContext executionContext,
            ILogger log)
            => Simulate<ApprenticeshipConfirmationConfirmedEvent>(req, executionContext, log);

        public async Task<IActionResult> Simulate<T>(HttpRequestMessage req, ExecutionContext executionContext, ILogger log)
        {
            try
            {
                var @event = JsonConvert.DeserializeObject<T>(await req.Content.ReadAsStringAsync());

                await endpoint.Publish(@event, executionContext, log);

                return new AcceptedResult();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }
    }
}
