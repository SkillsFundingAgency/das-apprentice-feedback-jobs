using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure
{
    public static class BlobProcessingExtension
    {
        public static void AddBlobProcessingServices(this IServiceCollection services)
        {
            services.AddTransient<IFeedbackTargetVariantBatchProcessor, FeedbackTargetVariantBatchProcessor>();
            services.AddTransient<IFeedbackTargetVariantBlobProcessor, FeedbackTargetVariantBlobProcessor>();
            services.AddTransient<IFeedbackTargetVariantBlobReader, FeedbackTargetVariantBlobReader>();
            services.AddTransient<IFeedbackTargetVariantBlobMover, FeedbackTargetVariantBlobMover>();
        }

    }
}