using RestEase;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public interface IApprenticeFeedbackApi
    {
        [Post("apprenticefeedbacktarget")]
        Task CreateFeedbackTarget([Body] ApprenticeConfirmedDetails apprenticeCommitment);

        [Get("feedbacktransaction?batchSize={batchSize}")]
        Task<IEnumerable<ApprenticeFeedbackTransaction>> GetFeedbackTransactionsToEmail([Path]int batchSize);

        [Post("feedbacktransaction/{id}")]
        Task<IEnumerable<ApprenticeFeedbackTransaction>> ProcessEmailTransaction([Path] Guid apprenticeFeedbackTransactionId);


        [Post("dataload/generate-feedback-summaries")]
        Task GenerateFeedbackSummaries();
    }
}