using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure
{
    public static class CreateTopicsAndQueues
    {
        public static async Task CreateQueuesAndTopics(
            IConfiguration configuration,
            string endpointQueueName,
            string connectionStringName = "AzureWebJobsServiceBus:fullyQualifiedNamespace",
            string topicName = "bundle-1",
            ILogger? logger = null)
        {
            var connectionString = configuration.GetValue<string>(connectionStringName);
            var adminClient = new ServiceBusAdministrationClient(connectionString, new DefaultAzureCredential());

            logger?.LogInformation("Queue Name: {queueName}", endpointQueueName);
            var errorQueue = $"{endpointQueueName}-error";

            await CreateTopic(topicName, adminClient, logger);
            await CreateQueue(endpointQueueName, adminClient, logger);
            await CreateQueue(errorQueue, adminClient, logger);

            await CreateSubscription(topicName, endpointQueueName, adminClient, endpointQueueName, logger);
        }

        private static async Task CreateTopic(string topicName, ServiceBusAdministrationClient adminClient, ILogger? logger)
        {
            if (await adminClient.TopicExistsAsync(topicName)) return;

            logger?.LogInformation("Creating topic: `{topicName}`", topicName);
            await adminClient.CreateTopicAsync(topicName);
        }

        private static async Task CreateQueue(string endpointQueueName, ServiceBusAdministrationClient adminClient, ILogger? logger)
        {
            if (await adminClient.QueueExistsAsync(endpointQueueName)) return;

            logger?.LogInformation("Creating queue: `{queueName}`", endpointQueueName);
            await adminClient.CreateQueueAsync(endpointQueueName);
        }

        private static async Task CreateSubscription(string topicName, string endpointName, ServiceBusAdministrationClient adminClient, string endpointQueueName, ILogger? logger)
        {
            if (await adminClient.SubscriptionExistsAsync(topicName, endpointQueueName)) return;

            logger?.LogInformation($"Creating subscription to: `{endpointQueueName}`", endpointQueueName);
            var createSubscriptionOptions = new CreateSubscriptionOptions(topicName, endpointName)
            {
                ForwardTo = endpointQueueName,
                UserMetadata = $"Subscribed to {endpointQueueName}"
            };
            var createRuleOptions = new CreateRuleOptions()
            {
                Filter = new FalseRuleFilter()
            };
            await adminClient.CreateSubscriptionAsync(createSubscriptionOptions, createRuleOptions);
        }
    }
}