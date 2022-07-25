
using RestEase;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System.Threading.Tasks;


namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public interface IApprenticeFeedbackEmailTransactionApi
    {
        [Post("feedbacktransaction")]
        Task<GetEmailTransactionsResponse> GenerateEmailTransactions([Body] GenerateEmailTransactionsRequest request);
    }
}
