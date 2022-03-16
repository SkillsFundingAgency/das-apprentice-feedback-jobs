// FunctionEndpointTrigger
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
	internal class FunctionEndpointTrigger
	{
		private readonly IFunctionEndpoint endpoint;

		public FunctionEndpointTrigger(IFunctionEndpoint endpoint)
		{
			this.endpoint = endpoint;
		}

		[FunctionName("ApprenticeFeedbackJobs")]
		public Task Run([ServiceBusTrigger("SFA.DAS.ApprenticeFeedback", AutoCompleteMessages = false)] ServiceBusReceivedMessage message, ServiceBusClient client, ServiceBusMessageActions messageActions, ILogger logger, ExecutionContext executionContext)
		{
			return endpoint.ProcessAtomic(message, executionContext, client, messageActions, logger);
		}
	}
}
