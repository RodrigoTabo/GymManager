using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Socios;

namespace GymManager.Client.ApiClients
{
    public class SocioApi(HttpClient HttpClient)
    {

        private readonly HttpClient _HttpClient = HttpClient;

        public async Task<List<SocioResponse>> ListarAsync()
            => await _HttpClient.GetJsonOrThrowAsync<List<SocioResponse>>($"api/socios");


        public async Task<int>CrearAsync(CreateSocioRequest request)
        {
            var crear = await _HttpClient.PostJsonOrThrowAsync<CreateSocioRequest, CreatedIdResponse>
                ("api/socios", request);

            return crear.Id;
        }

        public async Task<SocioResponse> GetByIdAsync(int id)
            => await _HttpClient.GetJsonOrThrowAsync<SocioResponse>($"api/socios/{id}");

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