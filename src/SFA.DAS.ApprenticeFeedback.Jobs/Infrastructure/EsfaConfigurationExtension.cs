using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure
{
    internal static class EsfaConfigurationExtension
    {
        public static void AddApplicationOptions(this IServiceCollection services)
        {
            services
                .AddOptions<ApplicationConfiguration>()
                .Configure<IConfiguration>((settings, configuration) =>
                    configuration.Bind(settings));
            services.AddSingleton(s => s.GetRequiredService<IOptions<ApplicationConfiguration>>().Value);
        }

        public static void ConfigureFromOptions<TOptions>(this IServiceCollection services, Func<ApplicationConfiguration, TOptions> func)
            where TOptions : class, new()
        {
            services.AddSingleton(s =>
                func(s.GetRequiredService<IOptions<ApplicationConfiguration>>().Value));
        }
    }
}