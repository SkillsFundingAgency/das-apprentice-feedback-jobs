using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Application.Services
{
    public class ApprenticeFeedbackService : IApprenticeFeedbackService
    {
        private readonly IApiClient _apiClient;

        public ApprenticeFeedbackService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task SendFeedbackEmails()
        {
            await _apiClient.Post(new PostSendFeedbackEmailsRequest());
        }
    }
}
