using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Extensions;
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
        Task ProcessBatch(List<FeedbackVariant> variants, int batchSize);
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

        public async Task ProcessBatch(List<FeedbackVariant> feedbackVariants, int batchSize)
        {
            var feedbackVariantBatches = feedbackVariants.ChunkBy(batchSize);
            bool isFirstBatch = true;

            foreach (var batch in feedbackVariantBatches)
            {
                bool clearStaging = isFirstBatch;
                isFirstBatch = false;

                bool isLastBatch = batch == feedbackVariantBatches.Last();

                var batchRequest = new PostProcessFeedbackVariantsRequest
                {
                    FeedbackTargetVariants = batch,
                    ClearStaging = clearStaging,
                    MergeStaging = isLastBatch
                };

                await _apprenticeFeedbackApi.ProcessFeedbackTargetVariants(batchRequest);

                _logger.LogInformation($"Processed batch with ItemCount : {batchRequest.FeedbackTargetVariants.Count}, ClearStaging: {clearStaging}, MergeStaging: {isLastBatch}");
            }
        }
    }

}
