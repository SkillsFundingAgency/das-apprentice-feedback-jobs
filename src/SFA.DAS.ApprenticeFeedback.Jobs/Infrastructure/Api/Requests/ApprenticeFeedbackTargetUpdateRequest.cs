using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class ApprenticeFeedbackTargetUpdateRequest
    {
        public Guid ApprenticeFeedbackTargetId { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
