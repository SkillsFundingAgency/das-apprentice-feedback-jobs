#nullable disable
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration
{
    public class ApprenticeFeedbackApiConfiguration : IApimClientConfiguration
    {
        public const string ApprenticeFeedbackApi = "ApprenticeFeedbackApi";
        public string ApiBaseUrl { get; set; }
        public string SubscriptionKey { get; set; }
        public string ApiVersion { get; set; }
    }
}
