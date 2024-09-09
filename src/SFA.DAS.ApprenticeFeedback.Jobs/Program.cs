using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeFeedback.Jobs.Extensions;

[assembly: NServiceBusTriggerFunction("SFA.DAS.ApprenticeFeedback.Jobs")]
var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder => builder.AddConfiguration())
    .ConfigureNServiceBus()
    .ConfigureServices((context, services) =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddServiceRegistrations(context.Configuration);

        services.AddLogging(options =>
        {
            options.AddApplicationInsights();
            options.AddFilter<ApplicationInsightsLoggerProvider>("SFA.DAS", LogLevel.Information);
            options.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);

        });
    })
    .Build();

host.Run();
