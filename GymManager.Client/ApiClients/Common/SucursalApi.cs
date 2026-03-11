using GymManager.Shared.Contracts.Sucursal;
using GymManager.Web.Security;

namespace GymManager.Client.ApiClients.Common
{
    public class SucursalApi(ApiHttpClientProvider clientProvider)
    {
        private readonly ApiHttpClientProvider _clientProvider = clientProvider;

        public async Task <List<SucursalResponse>> Get()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<List<SucursalResponse>>("api/sucursales");
        }

    }
}
