using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses
{
    public class ApprenticeFeedbackTransactionClick
    {
        public Guid ApprenticeFeedbackTargetId { get; set; }
        public string? LinkName { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime ClickedOn { get; set; }
    }
}
