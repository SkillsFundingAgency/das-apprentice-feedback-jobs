using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestEase.HttpClientFactory;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure;
using SFA.DAS.ApprenticeFeedback.Jobs.Services;
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

            services.Configure<FeedbackTargetVariantConfiguration>(configuration.GetSection(nameof(FeedbackTargetVariantConfiguration)));
            services.AddSingleton(sp =>
            {
                var feedbackTargetVariantConfig =
                    sp.GetRequiredService<IOptions<FeedbackTargetVariantConfiguration>>().Value;
                return new BlobServiceClient(feedbackTargetVariantConfig.BlobStorageConnectionString);
            });

            services.AddTransient<IWaveFanoutService>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<ApplicationConfiguration>>().Value;
                return new WaveFanoutService(config.EmailPerSecondCap, sp.GetRequiredService<ILogger<WaveFanoutService>>());
            });

            services.AddTransient<IFeedbackTargetVariantBatchProcessor, FeedbackTargetVariantBatchProcessor>();
            services.AddTransient<IFeedbackTargetVariantBlobProcessor, FeedbackTargetVariantBlobProcessor>();
            services.AddTransient<IFeedbackTargetVariantBlobReader, FeedbackTargetVariantBlobReader>();
            services.AddTransient<IFeedbackTargetVariantBlobMover, FeedbackTargetVariantBlobMover>();

            return services;
        }
    }
}
