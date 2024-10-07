using Dynamitey.DynamicObjects;
using System.Collections.Generic;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class PostProcessFeedbackVariantsRequest
    {
        public List<FeedbackVariant> FeedbackTargetVariants { get; set; } = new List<FeedbackVariant>();
        public bool ClearStaging { get; set; }
        public bool MergeStaging { get; set; }
    }

    public class FeedbackVariant
    {
        public long ApprenticeshipId { get; set; }
        public string Variant { get; set; }
        public FeedbackVariant(long apprenticeshipId, string variantName)
        { 
            ApprenticeshipId = apprenticeshipId;
            Variant = variantName;
        }
    }
}
