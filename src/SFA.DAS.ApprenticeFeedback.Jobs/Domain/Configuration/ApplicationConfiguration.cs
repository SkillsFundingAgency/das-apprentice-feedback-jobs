#nullable disable
namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration
{
    public class ApplicationConfiguration
    {
        public ApprenticeFeedbackApiConfiguration ApprenticeFeedbackOuterApi { get; set; }
        //public NServiceBusConfiguration NServiceBusConfiguration { get; set; }
        public FeedbackTargetVariantConfiguration FeedbackTargetVariantConfiguration { get; set; }
        public int EmailBatchSize { get; set; }
        public int UpdateBatchSize { get; set; }
    }
}
