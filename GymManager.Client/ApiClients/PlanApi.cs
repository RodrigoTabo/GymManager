using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Planes;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace GymManager.Client.ApiClients
{
    public class PlanApi
    {

        private readonly HttpClient _HttpClient;

        public PlanApi(HttpClient HttpClient)
        {
            _HttpClient = HttpClient;
        }


        public async Task<List<PlanResponse>> ListarAsync()
                => await _HttpClient.GetJsonOrThrowAsync<List<PlanResponse>>("api/planes");

        public async Task<int> CrearAsync(CreatePlanRequest request)
        {
            var crear = await _HttpClient.PostJsonOrThrowAsync<CreatePlanRequest, CreatedIdResponse>
                ("api/planes", request);

            return crear.Id;
        }

        public async Task UpdateAsync(int Id, UpdatePlanRequest request)
            => await _HttpClient.PutJsonOrThrowAsync($"api/planes/{Id}", request);

        public async Task SoftDeleteAsync(int Id)
            => await _HttpClient.DeleteOrThrowAsync($"api/planes/{Id}");

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }

    }
}
