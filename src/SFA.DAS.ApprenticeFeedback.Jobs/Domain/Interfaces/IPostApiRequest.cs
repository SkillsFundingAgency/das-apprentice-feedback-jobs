using Newtonsoft.Json;
namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces
{
    public interface IPostApiRequest
    {
        [JsonIgnore]
        string PostUrl { get; }
    }
}
