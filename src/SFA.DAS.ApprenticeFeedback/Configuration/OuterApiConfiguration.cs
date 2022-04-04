using SFA.DAS.Http.Configuration;

namespace SFA.DAS.ApprenticeFeedback.Configuration
{
    public class OuterApiConfiguration : IApimClientConfiguration
    {
        public string ApiBaseUrl { get; set; } = null!;
        public string SubscriptionKey { get; set; } = null!;
        public string ApiVersion { get; set; } = null!;
    }
}
