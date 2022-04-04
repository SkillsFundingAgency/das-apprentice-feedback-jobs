using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces
{
    public interface IApiClient
    {
        Task<TResponse> Get<TResponse>(IGetApiRequest request);
        Task Post(IPostApiRequest request);
    }
}
