using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses
{
    public enum EmailStatus
    {
         Successfull,
         NotAllowed,
         Failed
    }

    public class SendApprenticeFeedbackEmailResponse
    {
        public Guid ApprenticeFeedbackTransactionId { get; set; }
        public EmailStatus EmailStatus { get; set; }
    }
}
