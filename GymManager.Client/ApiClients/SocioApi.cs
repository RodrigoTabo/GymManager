using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Pagos;
using GymManager.Shared.Contracts.Socios;
using static System.Net.WebRequestMethods;

namespace GymManager.Client.ApiClients
{
    public class SocioApi(HttpClient HttpClient)
    {

        private readonly HttpClient _HttpClient = HttpClient;

        public async Task<List<SocioResponse>> ListarAsync(SocioQuery query)
        {
            var consulta = (query.Texto ?? "").Trim();

            var textoUrl = Uri.EscapeDataString(consulta);

            var url = string.IsNullOrEmpty(consulta)
                ? $"api/socios?inactivo={query.Inactivo}"
                : $"api/socios?buscarPor={query.BuscarPor}&texto={textoUrl}&inactivo={query.Inactivo}";

            return await _HttpClient.GetJsonOrThrowAsync<List<SocioResponse>>(url);
        }

        public async Task<int> CrearAsync(CreateSocioRequest request)
        {
            var crear = await _HttpClient.PostJsonOrThrowAsync<CreateSocioRequest, CreatedIdResponse>
                ("api/socios", request);

            return crear.Id;
        }

        public async Task<SocioResponse> GetByIdAsync(int id)
            => await _HttpClient.GetJsonOrThrowAsync<SocioResponse>($"api/socios/{id}");

        public async Task<SociosStatsResponse> GetStatsAsync()
            => await _HttpClient.GetJsonOrThrowAsync<SociosStatsResponse>($"api/socios/stats");

        public async Task UpdateAsync(int id, UpdateSocioRequest request)
            => await _HttpClient.PutJsonOrThrowAsync($"api/socios/{id}", request);

        public async Task SoftDeleteAsync(int id)
            => await _HttpClient.DeleteOrThrowAsync($"api/socios/{id}");

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }
    }
}