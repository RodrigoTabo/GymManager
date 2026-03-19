using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Socios;
using GymManager.Web.Security;

namespace GymManager.Client.ApiClients
{
    public class SocioApi(ApiHttpClientProvider clientProvider)
    {
        private readonly ApiHttpClientProvider _clientProvider = clientProvider;

        public async Task<List<SocioResponse>> ListarAsync(SocioQuery query)
        {
            var client = await _clientProvider.GetClientAsync();

            var consulta = (query.Texto ?? "").Trim();
            var textoUrl = Uri.EscapeDataString(consulta);

            var url = string.IsNullOrEmpty(consulta)
                ? $"api/socios?inactivo={query.Inactivo}"
                : $"api/socios?buscarPor={query.BuscarPor}&texto={textoUrl}&inactivo={query.Inactivo}";

            return await client.GetJsonOrThrowAsync<List<SocioResponse>>(url);
        }

        public async Task<int> CrearAsync(CreateSocioRequest request)
        {
            var client = await _clientProvider.GetClientAsync();

            var crear = await client.PostJsonOrThrowAsync<CreateSocioRequest, CreatedIdResponse>(
                $"api/socios", request);

            return crear.Id;
        }

        public async Task<SocioResponse> GetByIdAsync(int id)
        {
            var client = await _clientProvider.GetClientAsync();

            return await client.GetJsonOrThrowAsync<SocioResponse>(
                $"api/socios/{id}");
        }

        public async Task<SociosStatsResponse> GetStatsAsync()
        {
            var client = await _clientProvider.GetClientAsync();

            return await client.GetJsonOrThrowAsync<SociosStatsResponse>(
                $"api/socios/stats");
        }

        public async Task UpdateAsync(int id, UpdateSocioRequest request)
        {
            var client = await _clientProvider.GetClientAsync();

            await client.PutJsonOrThrowAsync(
                $"api/socios/{id}", request);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var client = await _clientProvider.GetClientAsync();

            await client.DeleteOrThrowAsync(
                $"api/socios/{id}");
        }

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }
    }
}