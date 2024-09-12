using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Extensions
{
    public static class AddServiceRegistrationsExtension
    {
        public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationOptions();
            services.ConfigureFromOptions(f => f.ApprenticeFeedbackOuterApi);

            services.AddSingleton<IApimClientConfiguration>(x => x.GetRequiredService<ApprenticeFeedbackApiConfiguration>());
            services.AddTransient<Http.MessageHandlers.DefaultHeadersHandler>();
            services.AddTransient<Http.MessageHandlers.LoggingMessageHandler>();
            services.AddTransient<Http.MessageHandlers.ApimHeadersHandler>();

            var url = services
                .BuildServiceProvider()
                .GetRequiredService<ApprenticeFeedbackApiConfiguration>()
                .ApiBaseUrl;

            services.AddRestEaseClient<IApprenticeFeedbackApi>(url)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();

            //services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
            return services;
        }

    }
}
