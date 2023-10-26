using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses
{
    public class FeedbackTransaction
    {
        public long FeedbackTransactionId { get; set; }
        public Guid ApprenticeId { get; set; }
        public Guid ApprenticeFeedbackTargetId { get; set; }
    }
}
