using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.ApprenticeFeedback.Jobs;
using SFA.DAS.ApprenticeFeedback.Jobs.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api;
using SFA.DAS.Configuration.AzureTableStorage;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SFA.DAS.ApprenticeFeedback.Jobs
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory());


            if (!configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
            {
                config.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = false;
                }
                );
            }

            var builtConfiguration = config.Build();

            builder.Services.AddOptions();
            builder.Services.Configure<ApprenticeFeedbackApiConfiguration>(builtConfiguration.GetSection(nameof(ApprenticeFeedbackApiConfiguration)));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ApprenticeFeedbackApiConfiguration>>().Value);

            builder.Services.AddHttpClient<IApiClient, ApiClient>();

            builder.Services.BuildServiceProvider();
        }
    }
}
