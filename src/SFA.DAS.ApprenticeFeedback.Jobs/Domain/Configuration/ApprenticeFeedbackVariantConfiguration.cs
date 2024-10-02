#nullable disable
namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration
{
    public class ApprenticeFeedbackVariantConfiguration
    {
        public string BlobContainerName { get; set; } = "apprentice-feedback-template-variants";
        public string IncomingFolder { get; set; } = "new";
        public string ArchiveFolder { get; set; } = "archive";
    }
}
