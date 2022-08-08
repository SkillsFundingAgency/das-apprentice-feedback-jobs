#nullable disable
namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration
{
    public class ApplicationConfiguration
    {
        public ApprenticeFeedbackApiConfiguration ApprenticeFeedbackOuterApi { get; set; }
        public NServiceBusConfiguration NServiceBusConfiguration { get; set; }
        public int EmailBatchSize { get; set; }
    }
}
