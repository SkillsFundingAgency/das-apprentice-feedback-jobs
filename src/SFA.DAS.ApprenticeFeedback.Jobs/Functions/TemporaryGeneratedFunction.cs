//// FunctionEndpointTrigger
//using Azure.Messaging.ServiceBus;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Extensions.Logging;
//using Microsoft.Azure.Functions.Worker;

//namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
//{
//    /// <summary>
//    /// This function exists due to the constraint in AzureDevOps whereby only the .Net Core 3.1 SDK's are available.
//    /// As a result, the NService bus pacakges that rely on the .Net 5 SDK to generate source code don't run.
//    /// Therefore until our agents are upgrades, the function has been manually dropped in for now.
//    /// </summary>
//    internal class FunctionEndpointTrigger
//    {
//        private readonly IFunctionEndpoint endpoint;

//        public FunctionEndpointTrigger(IFunctionEndpoint endpoint)
//        {
//            this.endpoint = endpoint;
//        }

//        [FunctionName("ApprenticeFeedbackJobs")]
//        public Task Run([ServiceBusTrigger("SFA.DAS.ApprenticeFeedback", AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions, ILogger logger, ExecutionContext executionContext)
//        {
//            return endpoint.ProcessAtomic(message, executionContext, client, messageActions, logger);
//        }
//    }
//}
