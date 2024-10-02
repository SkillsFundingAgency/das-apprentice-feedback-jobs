using Dynamitey.DynamicObjects;
using System.Collections.Generic;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class PostProcessFeedbackVariantsRequest
    {
        public List<FeedbackVariant> FeedbackVariants { get; set; } = new List<FeedbackVariant>();
    }

    public class FeedbackVariant
    {
        public long ApprenticeshipId { get; set; }
        public string VariantName { get; set; }
        public FeedbackVariant(long apprenticeshipId, string variantName)
        { 
            ApprenticeshipId = apprenticeshipId;
            VariantName = variantName;
        }
    }
}
