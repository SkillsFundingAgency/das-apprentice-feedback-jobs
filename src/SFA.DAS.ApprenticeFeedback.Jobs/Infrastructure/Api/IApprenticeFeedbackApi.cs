using RestEase;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Models;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public interface IApprenticeFeedbackApi
    {
        [Post("apprenticefeedbacktarget")]
        Task CreateFeedbackTarget([Body] ApprenticeConfirmedDetails apprenticeCommitment);

        [Post("dataload/generate-feedback-summaries")]
        Task GenerateFeedbackSummaries();

        [Get("feedbacktransaction?batchSize={batchSize}")]
        Task<IEnumerable<ApprenticeFeedbackTransaction>> GetFeedbackTransactionsToEmail([Path] int batchSize);

        [Post("feedbacktransaction/{id}")]
        Task<SendApprenticeFeedbackEmailResponse> ProcessEmailTransaction([Path] long id, [Body] ApprenticeFeedbackTransaction apprenticeFeedbackTransaction);
        [Get("apprenticefeedbacktarget/requiresupdate")]
        Task<List<FeedbackTargetForUpdate>> GetFeedbackTargetsForUpdate(int batchSize);

        [Put("apprenticefeedbacktarget")]
        Task<ApprenticeFeedbackTargetUpdateResponse> UpdateFeedbackTarget([Body] ApprenticeFeedbackTargetUpdateRequest apprenticeFeedbackTargetToUpdate);
        [Post("feedbacktransaction")]
        Task<GetEmailTransactionsResponse> GenerateEmailTransactions();
    }
}