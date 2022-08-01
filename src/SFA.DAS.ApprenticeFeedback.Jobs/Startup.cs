﻿
using System;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure;
using SFA.DAS.Http.Configuration;


[assembly: FunctionsStartup(typeof(Startup))]
// Commenting out the AutoGeneratedFunction until the agents can build with .Net 5 SDK, otherwise it fails to generated in the Azure DevOps pipeline.
//[assembly: NServiceBusTriggerFunction(Startup.EndpointName, SendsAtomicWithReceive = true, TriggerFunctionName = "ApprenticeFeedbackJobs")]

namespace SFA.DAS.ApprenticeFeedback.Jobs
{
    public class Startup : FunctionsStartup
    {
        internal const string EndpointName = "SFA.DAS.ApprenticeFeedback";

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.ConfigureConfiguration();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.ConfigureLogging();

            var logger = LoggerFactory.Create(b => b.ConfigureLogging()).CreateLogger<Startup>();

            CreateTopicsAndQueues.CreateQueuesAndTopics(builder.GetContext().Configuration, EndpointName, logger: logger)
                .GetAwaiter().GetResult();

            builder.UseNServiceBus((IConfiguration appConfiguration) =>
            {
                var configuration = new ServiceBusTriggeredEndpointConfiguration(EndpointName);
                var connectionStringConfiguration = ServiceBusConnectionConfiguration.GetServiceBusConnectionString(appConfiguration);
                if (connectionStringConfiguration.ConnectionType == ServiceBusConnectionConfiguration.ConnectionAuthenticationType.ManagedIdentity)
                {
                    configuration.Transport.ConnectionString(connectionStringConfiguration.ConnectionString);
                    configuration.Transport.CustomTokenCredential(new DefaultAzureCredential());
                }
                else
                {
                    //Shared Access Key, Will pick up the AzureServiceJobsServiceBus Setting by Default.
                }

                var nServiceBusConfig = appConfiguration.GetSection("NServiceBusConfiguration").Get<NServiceBusConfiguration>();
                if (!string.IsNullOrWhiteSpace(nServiceBusConfig.License))
                {
                    configuration.AdvancedConfiguration.License(nServiceBusConfig.License);
                }

                configuration.AdvancedConfiguration.SendFailedMessagesTo($"{EndpointName}-error");
                configuration.LogDiagnostics();

                configuration.AdvancedConfiguration.Conventions()
                    .DefiningMessagesAs(IsMessage)
                    .DefiningEventsAs(IsEvent)
                    .DefiningCommandsAs(IsCommand);

                configuration.Transport.SubscriptionRuleNamingConvention(AzureQueueNameShortener.Shorten);

                configuration.AdvancedConfiguration.Pipeline.Register(new LogIncomingBehaviour(), nameof(LogIncomingBehaviour));
                configuration.AdvancedConfiguration.Pipeline.Register(new LogOutgoingBehaviour(), nameof(LogOutgoingBehaviour));

                var persistence = configuration.AdvancedConfiguration.UsePersistence<AzureTablePersistence>();
                persistence.ConnectionString(appConfiguration.GetConnectionStringOrSetting("AzureWebJobsStorage"));
                configuration.AdvancedConfiguration.EnableInstallers();

                return configuration;
            });

            builder.Services.AddApplicationOptions();
            builder.Services.ConfigureFromOptions(f => f.ApprenticeFeedbackOuterApi);
            builder.Services.AddSingleton<IApimClientConfiguration>(x => x.GetRequiredService<ApprenticeFeedbackApiConfiguration>());
            builder.Services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
            builder.Services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();

            var url = builder.Services
                .BuildServiceProvider()
                .GetRequiredService<ApprenticeFeedbackApiConfiguration>()
                .ApiBaseUrl;

            builder.Services.AddRestEaseClient<IApprenticeFeedbackApi>(url)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();
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
