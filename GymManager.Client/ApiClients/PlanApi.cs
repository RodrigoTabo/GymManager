using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Planes;
using GymManager.Web.Security;

namespace GymManager.Client.ApiClients
{
    public class PlanApi(ApiHttpClientProvider clientProvider)
    {
        private readonly ApiHttpClientProvider _clientProvider = clientProvider;

        public async Task<List<PlanResponse>> ListarAsync()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<List<PlanResponse>>(
                $"api/planes");
        }

        public async Task<StatsPlanRequest> GetPlanStatsAsync()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<StatsPlanRequest>(
                $"api/planes/stats");
        }

        public async Task<int> CrearAsync(CreatePlanRequest request)
        {
            var client = await _clientProvider.GetClientAsync();

            var crear = await client.PostJsonOrThrowAsync<CreatePlanRequest, CreatedIdResponse>(
                $"api/planes", request);

            return crear.Id;
        }

        public async Task<PlanResponse> GetByIdAsync(int id)
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<PlanResponse>(
                $"api/planes/{id}");
        }

        public async Task UpdateAsync(int id, UpdatePlanRequest request)
        {
            var client = await _clientProvider.GetClientAsync();
            await client.PutJsonOrThrowAsync($"api/planes/{id}", request);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var client = await _clientProvider.GetClientAsync();
            await client.DeleteOrThrowAsync(
                $"api/planes/{id}");
        }

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }
    }
}
