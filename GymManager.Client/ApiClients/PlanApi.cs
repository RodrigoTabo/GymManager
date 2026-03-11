using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Planes;
using GymManager.Web.Security;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace GymManager.Client.ApiClients
{
    public class PlanApi(ApiHttpClientProvider clientProvider)
    {
        private readonly ApiHttpClientProvider _clientProvider = clientProvider;

        public async Task<List<PlanResponse>> ListarAsync(Guid sucursalId)
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<List<PlanResponse>>(
                $"api/sucursales/{sucursalId}/planes");
        }

        public async Task<StatsPlanRequest> GetPlanStatsAsync(Guid sucursalId)
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<StatsPlanRequest>(
                $"api/sucursales/{sucursalId}/planes/stats");
        }

        public async Task<int> CrearAsync(Guid sucursalId, CreatePlanRequest request)
        {
            var client = await _clientProvider.GetClientAsync();

            var crear = await client.PostJsonOrThrowAsync<CreatePlanRequest, CreatedIdResponse>(
                $"api/sucursales/{sucursalId}/planes", request);

            return crear.Id;
        }

        public async Task<PlanResponse> GetByIdAsync(Guid sucursalId, int id)
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<PlanResponse>(
                $"api/sucursales/{sucursalId}/planes/{id}");
        }

        public async Task UpdateAsync(Guid sucursalId, int id, UpdatePlanRequest request)
        {
            var client = await _clientProvider.GetClientAsync();
            await client.PutJsonOrThrowAsync(
                $"api/sucursales/{sucursalId}/planes/{id}", request);
        }

        public async Task SoftDeleteAsync(Guid sucursalId, int id)
        {
            var client = await _clientProvider.GetClientAsync();
            await client.DeleteOrThrowAsync(
                $"api/sucursales/{sucursalId}/planes/{id}");
        }

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }
    }
}
