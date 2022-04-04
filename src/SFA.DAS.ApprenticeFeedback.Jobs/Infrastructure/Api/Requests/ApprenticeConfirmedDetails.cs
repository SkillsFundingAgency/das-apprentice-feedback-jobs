using SFA.DAS.ApprenticeCommitments.Messages.Events;
using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests
{
    public class ApprenticeConfirmedDetails
    {
        public Guid ApprenticeId { get; set; }
        public long ApprenticeshipId { get; set; }
        public long ConfirmationId { get; set; }
        public DateTime ConfirmedOn { get; set; }
        public long CommitmentsApprenticeshipId { get; set; }
        public DateTime CommitmentsApprovedOn { get; set; }

        public static implicit operator ApprenticeConfirmedDetails(ApprenticeshipConfirmationConfirmedEvent input)
        {
            return new ApprenticeConfirmedDetails
            {
                ApprenticeId = input.ApprenticeId,
                ConfirmationId = input.ConfirmationId,
                ConfirmedOn = input.ConfirmedOn,
                ApprenticeshipId = input.ApprenticeshipId,
                CommitmentsApprenticeshipId = input.CommitmentsApprenticeshipId,
                CommitmentsApprovedOn = input.CommitmentsApprovedOn
            };
        }
    }
}
