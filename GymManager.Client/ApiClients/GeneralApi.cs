using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.General;

namespace GymManager.Client.ApiClients
{
    public class GeneralApi(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;


        public async Task<GeneralResponse> GetStatsAsync()
            => await _httpClient.GetJsonOrThrowAsync<GeneralResponse>("api/general");

    }
}
