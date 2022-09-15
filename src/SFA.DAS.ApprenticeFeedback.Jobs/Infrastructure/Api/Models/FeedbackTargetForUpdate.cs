using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Models
{
    public class FeedbackTargetForUpdate
    {
        public Guid ApprenticeFeedbackTargetId { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
