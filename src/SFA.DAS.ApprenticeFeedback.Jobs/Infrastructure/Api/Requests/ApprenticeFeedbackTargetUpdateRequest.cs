using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class UpdateApprenticeFeedbackTargetRequest
    {
        public Guid ApprenticeFeedbackTargetId { get; set; }
        public Guid ApprenticeId { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
