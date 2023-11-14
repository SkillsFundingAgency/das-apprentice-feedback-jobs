using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class FeedbackTransactionClickRequest
    {
        public long FeedbackTransactionId { get; set; }
        public Guid ApprenticeFeedbackTargetId { get; set; }
        public string? LinkName { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime ClickedOn { get; set; }
    }
}
