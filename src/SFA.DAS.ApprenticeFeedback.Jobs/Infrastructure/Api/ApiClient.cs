using Microsoft.Extensions.Options;
using SFA.DAS.ApprenticeFeedback.Jobs.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _client;
        private readonly ApprenticeFeedbackApiConfiguration _config;

        public ApiClient(HttpClient client, IOptions<ApprenticeFeedbackApiConfiguration> configOptions)
        {
            _client = client;
            _config = configOptions.Value;

            _client.BaseAddress = new Uri(_config.ApiBaseUrl);
        }

        public Task<TResponse> Get<TResponse>(IGetApiRequest request)
        {
            AddHeaders();

            throw new System.NotImplementedException();
        }

        public async Task Post(IPostApiRequest request)
        {
            AddHeaders();

            var response = await _client.PostAsync(request.PostUrl, null)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }

        private void AddHeaders()
        {
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _config.ApiBaseUrl);
            _client.DefaultRequestHeaders.Add("X-Version", "1");
        }
    }
}
