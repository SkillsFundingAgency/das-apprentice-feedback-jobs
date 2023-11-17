using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Models;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses
{
    public class UpdateApprenticeFeedbackTargetResponse
    {
        public FeedbackTargetForUpdate ApprenticeFeedbackTarget { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
