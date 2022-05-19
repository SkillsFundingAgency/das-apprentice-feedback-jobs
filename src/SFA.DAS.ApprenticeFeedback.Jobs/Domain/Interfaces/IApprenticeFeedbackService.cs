using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces
{
    public interface IApprenticeFeedbackService
    {
        Task SendFeedbackEmails();
    }
}
