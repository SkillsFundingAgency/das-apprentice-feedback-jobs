using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses
{
    public class ApprenticeFeedbackTransaction
    {
        public long ApprenticeFeedbackTransactionId { get; set; }
        public Guid ApprenticeId { get; set; }
    }
}
