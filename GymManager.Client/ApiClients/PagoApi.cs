using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Pagos;

namespace GymManager.Client.ApiClients
{
    public class PagoApi(HttpClient HttpClient)
    {
        private readonly HttpClient _HttpClient = HttpClient;


        public async Task<List<PagoResponse>> ListarAsync()
            => await _HttpClient.GetJsonOrThrowAsync<List<PagoResponse>>($"api/pagos");

        public async Task<PagosStatsResponse> GetPagosStatsAsync()
            => await _HttpClient.GetJsonOrThrowAsync<PagosStatsResponse>($"api/pagos/stats");

        public async Task<int> CrearAsync(CreatePagoRequest request)
        {
            var crear = await _HttpClient.PostJsonOrThrowAsync<CreatePagoRequest, CreatedIdResponse>
                ("api/pagos", request);

            return crear.Id;
        }

        public async Task UpdateAsync(UpdatePagoRequest request, int id)
            => await _HttpClient.PutJsonOrThrowAsync($"api/pago/{id}", request);

        public async Task SoftDeleteAsync(int id)
            => await _HttpClient.DeleteOrThrowAsync($"api/pagos/{id}");

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }

    }
}
