using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.ApprenticeFeedback.Jobs.Extensions;

[assembly: NServiceBusTriggerFunction("SFA.DAS.ApprenticeFeedback.Jobs")]

var configBuilder = new ConfigurationBuilder();
var configuration = configBuilder.AddConfiguration().Build();

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder => builder.AddConfiguration(configuration))
    .ConfigureNServiceBus(configuration)
    .ConfigureServices((context, services) =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddServiceRegistrations(context.Configuration);

        services.AddOpenTelemetryRegistration(context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]!);
    })
    .Build();

await host.RunAsync();