using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.General;
using GymManager.Web.Security;

namespace GymManager.Client.ApiClients
{
    public class GeneralApi(ApiHttpClientProvider clientProvider)
    {

        private readonly ApiHttpClientProvider _clientProvider = clientProvider;


        public async Task<GeneralResponse> GetStatsAsync()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<GeneralResponse>("api/general");
        }

    }
}
