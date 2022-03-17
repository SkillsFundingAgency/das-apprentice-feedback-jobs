using RestEase;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public interface IApprenticeFeedbackApi
    {
        [Post("feedbacktarget")]
        Task CreateFeedbackTarget([Body] ApprenticeConfirmedDetails apprenticeCommitment);
    }
}