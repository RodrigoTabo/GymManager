using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Pagos;

namespace GymManager.Client.ApiClients
{
    public class PagoApi(HttpClient HttpClient)
    {
        private readonly HttpClient _HttpClient = HttpClient;

        public async Task<int> CrearAsync(CreatePagoRequest request)
        {
            var crear = await _HttpClient.PostJsonOrThrowAsync<CreatePagoRequest, CreatedIdResponse>
                ("api/pagos", request);

            return crear.Id;
        }

        private class CreatedIdResponse
        {
            public int Id { get; set; }
        }

    }
}
