using Newtonsoft.Json;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces
{
    public interface IGetApiRequest
    {
        [JsonIgnore]
        string GetUrl { get; }
    }
}
