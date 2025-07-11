using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Extensions
{
    public static class AddConfigurationExtension
    {
        public static IConfigurationBuilder AddConfiguration(this IConfigurationBuilder builder)
        {
            var preConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            builder.AddConfiguration(preConfig);

            builder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = preConfig["ConfigNames"]!
                    .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                options.StorageConnectionString = preConfig["ConfigurationStorageConnectionString"];
                options.EnvironmentName = preConfig["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            return builder;
        }
    }
}
