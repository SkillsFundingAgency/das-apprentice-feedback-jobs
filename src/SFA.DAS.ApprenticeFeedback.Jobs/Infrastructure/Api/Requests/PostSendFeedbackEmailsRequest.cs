using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class PostSendFeedbackEmailsRequest : IPostApiRequest
    {
        public string PostUrl => "apprentice-feedback/send-emails";
    }
}
