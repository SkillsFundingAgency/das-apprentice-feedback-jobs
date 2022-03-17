using AutoFixture;
using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests
{
    public class WhenApprenticeshipDetailsHaveBeenConfirmed
    {
        private readonly Fixture _fixture = new Fixture();

        [Test, AutoMoqData]
        public async Task And_it_is_a_new_apprenticeship_Then_create_the_apprentice_record(
            [Frozen] Mock<IApprenticeFeedbackApi> api,
            ApprenticeshipConfirmationCommencedEventHandler sut)
        {
            var evt = _fixture.Build<ApprenticeshipConfirmationConfirmedEvent>()
               .Create();

            await sut.Handle(evt, new TestableMessageHandlerContext());

            api.Verify(m => m.CreateFeedbackTarget(It.Is<ApprenticeConfirmedDetails>(n =>
            n.ApprenticeId == evt.ApprenticeId &&
            n.ApprenticeshipId == evt.ApprenticeshipId &&
            n.CommitmentsApprenticeshipId == evt.CommitmentsApprenticeshipId &&
            n.CommitmentsApprovedOn == evt.CommitmentsApprovedOn &&
            n.ConfirmationId == evt.ConfirmationId &&
            n.ConfirmedOn == evt.ConfirmedOn)));
        }
    }
}