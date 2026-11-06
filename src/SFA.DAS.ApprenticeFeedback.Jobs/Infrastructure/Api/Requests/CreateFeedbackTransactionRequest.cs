namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class CreateFeedbackTransactionRequest
    {
        public long ApprenticeshipId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
    }
}
