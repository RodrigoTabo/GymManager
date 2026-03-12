using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Pagos;
using GymManager.Web.Security;

namespace GymManager.Client.ApiClients
{
    public class PagoApi(ApiHttpClientProvider clientProvider)
    {

        private readonly ApiHttpClientProvider _clientProvider = clientProvider;

        public async Task<List<PagoResponse>> ListarAsync(Guid sucursalId)
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<List<PagoResponse>>($"api/sucursales/{sucursalId}/pagos");
        }

        public async Task<PagosStatsResponse> GetPagosStatsAsync(Guid sucursalId)
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<PagosStatsResponse>($"api/sucursales/{sucursalId}/pagos/stats");
        }

        public async Task<int> CrearAsync(Guid sucursalId, CreatePagoRequest request)
        {
            var client = await _clientProvider.GetClientAsync();

            var crear = await client.PostJsonOrThrowAsync<CreatePagoRequest, CreatedIdResponse>
                ($"api/sucursales/{sucursalId}/pagos", request);

            return crear.Id;
        }

        public async Task UpdateAsync(Guid sucursalId, UpdatePagoRequest request, int id)
        {
            var client = await _clientProvider.GetClientAsync();
            await client.PutJsonOrThrowAsync($"api/sucursales/{sucursalId}/pagos/{id}", request);
        }

        public async Task SoftDeleteAsync(Guid sucursalId, int id)
        {
            var client = await _clientProvider.GetClientAsync();
            await client.DeleteOrThrowAsync($"api/sucursales/{sucursalId}/pagos/{id}");
        }

        public async Task<List<VencidoResponse>> GetVencidosAsync(Guid sucursalId)
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<List<VencidoResponse>>($"api/sucursales/{sucursalId}/pagos/vencidos");
        }

        public async Task<VencimientoStatsResponse> GetVencimientoStatsAsync(Guid sucursalId)
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<VencimientoStatsResponse>($"api/sucursales/{sucursalId}/pagos/vencidos/stats");
        }

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }

    }
}
