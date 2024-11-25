using Microsoft.Extensions.Hosting;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure;
using System.Net;
using System.Text.RegularExpressions;
using Azure.Messaging.ServiceBus;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Extensions
{
    public static class ConfigureNServiceBusExtension
    {
        const string EndpointName = $"SFA.DAS.ApprenticeFeedback.Jobs";

        public static IHostBuilder ConfigureNServiceBus(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseNServiceBus((configuration, endpointConfiguration) =>
            {
                endpointConfiguration.Transport.SubscriptionRuleNamingConvention = AzureQueueNameShortener.Shorten;

                endpointConfiguration.AdvancedConfiguration.EnableInstallers();

                endpointConfiguration.AdvancedConfiguration.Conventions()
                    .DefiningCommandsAs(t => Regex.IsMatch(t.Name, "Command(V\\d+)?$"))
                    .DefiningEventsAs(t => Regex.IsMatch(t.Name, "Event(V\\d+)?$"));

                endpointConfiguration.AdvancedConfiguration.SendFailedMessagesTo($"{EndpointName}-error");

                endpointConfiguration.AdvancedConfiguration.Conventions()
                    .DefiningMessagesAs(IsMessage)
                    .DefiningEventsAs(IsEvent)
                    .DefiningCommandsAs(IsCommand);

                var persistence = endpointConfiguration.AdvancedConfiguration.UsePersistence<AzureTablePersistence>();
                persistence.ConnectionString(configuration["AzureWebJobsStorage"]);

                var decodedLicence = WebUtility.HtmlDecode(configuration["NServiceBusConfiguration:License"]);
                endpointConfiguration.AdvancedConfiguration.License(decodedLicence);
            });
            return hostBuilder;
        }

        private static bool IsMessage(Type t) => t is IMessage || IsSfaMessage(t, "Messages");

        private static bool IsEvent(Type t) => t is IEvent || IsSfaMessage(t, "Messages.Events");

        private static bool IsCommand(Type t) => t is ICommand || IsSfaMessage(t, "Messages.Commands");

        private static bool IsSfaMessage(Type t, string namespaceSuffix)
            => t.Namespace != null &&
               t.Namespace.StartsWith("SFA.DAS") &&
               t.Namespace.EndsWith(namespaceSuffix);
    }
}



