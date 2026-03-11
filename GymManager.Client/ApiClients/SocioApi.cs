using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Pagos;
using GymManager.Shared.Contracts.Socios;
using GymManager.Web.Security;
using static System.Net.WebRequestMethods;

namespace GymManager.Client.ApiClients
{
    public class SocioApi(ApiHttpClientProvider clientProvider)
    {
        private readonly ApiHttpClientProvider _clientProvider = clientProvider;

        public async Task<List<SocioResponse>> ListarAsync(Guid sucursalId, SocioQuery query)
        {
            var client = await _clientProvider.GetClientAsync();

            var consulta = (query.Texto ?? "").Trim();
            var textoUrl = Uri.EscapeDataString(consulta);

            var url = string.IsNullOrEmpty(consulta)
                ? $"api/sucursales/{sucursalId}/socios?inactivo={query.Inactivo}"
                : $"api/sucursales/{sucursalId}/socios?buscarPor={query.BuscarPor}&texto={textoUrl}&inactivo={query.Inactivo}";

            return await client.GetJsonOrThrowAsync<List<SocioResponse>>(url);
        }

        public async Task<int> CrearAsync(Guid sucursalId, CreateSocioRequest request)
        {
            var client = await _clientProvider.GetClientAsync();

            var crear = await client.PostJsonOrThrowAsync<CreateSocioRequest, CreatedIdResponse>(
                $"api/sucursales/{sucursalId}/socios", request);

            return crear.Id;
        }

        public async Task<SocioResponse> GetByIdAsync(Guid sucursalId, int id)
        {
            var client = await _clientProvider.GetClientAsync();

            return await client.GetJsonOrThrowAsync<SocioResponse>(
                $"api/sucursales/{sucursalId}/socios/{id}");
        }

        public async Task<SociosStatsResponse> GetStatsAsync(Guid sucursalId)
        {
            var client = await _clientProvider.GetClientAsync();

            return await client.GetJsonOrThrowAsync<SociosStatsResponse>(
                $"api/sucursales/{sucursalId}/socios/stats");
        }

        public async Task UpdateAsync(Guid sucursalId, int id, UpdateSocioRequest request)
        {
            var client = await _clientProvider.GetClientAsync();

            await client.PutJsonOrThrowAsync(
                $"api/sucursales/{sucursalId}/socios/{id}", request);
        }

        public async Task SoftDeleteAsync(Guid sucursalId, int id)
        {
            var client = await _clientProvider.GetClientAsync();

            await client.DeleteOrThrowAsync(
                $"api/sucursales/{sucursalId}/socios/{id}");
        }

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }
    }
}