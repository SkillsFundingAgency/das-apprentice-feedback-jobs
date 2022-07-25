using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class PostGenerateEmailTransactionsRequest : IPostApiRequest
    {
        public string PostUrl => "feedbacktransaction";
    }
}
