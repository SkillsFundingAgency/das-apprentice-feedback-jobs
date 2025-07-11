#nullable disable
namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration
{
    public class FeedbackTargetVariantConfiguration
    {
        public string BlobStorageConnectionString { get; set; }
        public string BlobContainerName { get; set; }
        public string ArchiveFolder { get; set; }
        public int FeedbackTargetVariantBatchSize { get; set; }
    }
}