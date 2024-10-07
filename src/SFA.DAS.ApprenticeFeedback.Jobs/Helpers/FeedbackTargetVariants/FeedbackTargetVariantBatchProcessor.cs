using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants
{
    public interface IFeedbackTargetVariantBatchProcessor
    {
        Task ProcessBatch(IEnumerable<FeedbackVariant> variants, int batchSize);
    }
    public class FeedbackTargetVariantBatchProcessor : IFeedbackTargetVariantBatchProcessor
    {
        private readonly ILogger<FeedbackTargetVariantBatchProcessor> _logger;
        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;

        public FeedbackTargetVariantBatchProcessor(ILogger<FeedbackTargetVariantBatchProcessor> logger, IApprenticeFeedbackApi apprenticeFeedbackApi)
        {
            _logger = logger;
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
        }

        public async Task ProcessBatch(IEnumerable<FeedbackVariant> feedbackVariants, int batchSize)
        {
            var variantsList = feedbackVariants.ToList();
            bool isFirstBatch = true;

            for (int i = 0; i < variantsList.Count; i += batchSize)
            {
                var batch = variantsList.Skip(i).Take(batchSize).ToList();
                bool clearStaging = isFirstBatch;
                isFirstBatch = false;

                bool isLastBatch = i + batchSize >= variantsList.Count;

                var batchRequest = new PostProcessFeedbackVariantsRequest
                {
                    FeedbackTargetVariants = batch,
                    ClearStaging = clearStaging,
                    MergeStaging = isLastBatch
                };

                await _apprenticeFeedbackApi.ProcessFeedbackTargetVariants(batchRequest);
                _logger.LogInformation($"Processed batch with ClearStaging: {clearStaging}, MergeStaging: {isLastBatch}");
            }
        }
    }

}
