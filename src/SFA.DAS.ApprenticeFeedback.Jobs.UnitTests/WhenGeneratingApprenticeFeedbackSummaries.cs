using AutoFixture.NUnit3;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Messages.Commands;
using SFA.DAS.ApprenticeFeedback.Jobs.Handlers.ApprenticeCommitmentsHandlers;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests
{
    public class WhenGeneratingApprenticeFeedbackSummaries
    {
        [Test, AutoMoqData]
        public async Task And_it_is_a_new_apprenticeship_Then_create_the_apprentice_record(
            [Frozen] Mock<IApprenticeFeedbackApi> api,
            GenerateApprenticeFeedbackSummariesCommand command,
            GenerateApprenticeFeedbackSummariesCommandHandler sut)
        {

            await sut.Handle(command, new TestableMessageHandlerContext());

            api.Verify(m => m.GenerateFeedbackSummaries(), Times.Once);
        }
    }
}