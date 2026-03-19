using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Pagos;
using GymManager.Web.Security;

namespace GymManager.Client.ApiClients
{
    public class PagoApi(ApiHttpClientProvider clientProvider)
    {

        private readonly ApiHttpClientProvider _clientProvider = clientProvider;

        public async Task<List<PagoResponse>> ListarAsync()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<List<PagoResponse>>($"api/pagos");
        }

        public async Task<PagosStatsResponse> GetPagosStatsAsync()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<PagosStatsResponse>($"api/pagos/stats");
        }

        public async Task<int> CrearAsync(CreatePagoRequest request)
        {
            var client = await _clientProvider.GetClientAsync();

            var crear = await client.PostJsonOrThrowAsync<CreatePagoRequest, CreatedIdResponse>
                ($"api/pagos", request);

            return crear.Id;
        }

        public async Task UpdateAsync( UpdatePagoRequest request, int id)
        {
            var client = await _clientProvider.GetClientAsync();
            await client.PutJsonOrThrowAsync($"api/pagos/{id}", request);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var client = await _clientProvider.GetClientAsync();
            await client.DeleteOrThrowAsync($"api/pagos/{id}");
        }

        public async Task<List<VencidoResponse>> GetVencidosAsync()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<List<VencidoResponse>>($"api/pagos/vencidos");
        }

        public async Task<VencimientoStatsResponse> GetVencimientoStatsAsync()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<VencimientoStatsResponse>($"api/pagos/vencidos/stats");
        }

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }

    }
}
