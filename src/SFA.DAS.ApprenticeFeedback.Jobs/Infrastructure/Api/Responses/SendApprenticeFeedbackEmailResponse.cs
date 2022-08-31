using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses
{
    public enum EmailStatus
    {
         Successful,
         NotAllowed,
         Failed
    }

    public class SendApprenticeFeedbackEmailResponse
    {
        public long FeedbackTransactionId { get; set; }
        public EmailStatus EmailStatus { get; set; }
    }
}
