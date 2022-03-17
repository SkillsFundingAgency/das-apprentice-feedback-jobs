using RestEase;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Jobs.Api
{
    public interface IApprenticeFeedbackApi
    {
        [Post("apprenticeCommitments")]
        Task CreateFeedbackTarget([Body] ApprenticeConfirmedDetails apprenticeCommitment);

        //[Post("approvals")]
        //Task CreateApproval([Body] ApprovalCreated approval);

        //[Put("approvals")]
        //Task UpdateApproval([Body] ApprovalUpdated approval);

        //[Post("approvals/stopped")]
        //Task StopApprenticeship([Body] ApprovalStopped approval);

        //[Get("approvals/{commitmentsApprenticeshipId}/registration")]
        //Task<Registration> GetApprovalsRegistration([Path]long commitmentsApprenticeshipId);

        //[Get("registrations/{id}")]
        //Task<Registration> GetRegistration([Path] Guid id);

        //[Get("/apprentices/{apprenticeid}")]
        //Task<Api.Apprentice> GetApprentice([Path] Guid apprenticeid);

        //[Get("/apprentices/{apprenticeid}/apprenticeships")]
        //Task<ApprenticeshipsWrapper> GetApprenticeships([Path] Guid apprenticeid);

        //[Get("/apprentices/{apprenticeid}/apprenticeships/{apprenticeshipid}/revisions")]
        //Task<ApprenticeshipHistory> GetApprenticeshipHistory([Path] Guid apprenticeid, [Path] long apprenticeshipid);

        //[Post("registrations/reminders")]
        //Task SendInvitationReminders([Body] SendInvitationRemindersRequest request);

        //[Get("registrations/reminders")]
        //Task<RegistrationsWrapper> GetReminderRegistrations([Query] DateTime invitationCutOffTime);

        //[Post("registrations/{apprenticeId}/reminder")]
        //Task InvitationReminderSent([Path] Guid apprenticeId, [Body] RegistrationReminderSentRequest request);

        //[Patch("/apprentices/{apprenticeId}")]
        //Task UpdateApprentice([Path] Guid apprenticeId, [Body] JsonPatchDocument<Apprentice> patch);
    }
}