using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.MetodoPago;

namespace GymManager.Client.ApiClients
{
    public class MetodoPagoApi(HttpClient httpClient)
    {

        private readonly HttpClient _httpClient = httpClient;


        public async Task<List<MetodoPagoResponse>> ListarAsync()
            => await _httpClient.GetJsonOrThrowAsync<List<MetodoPagoResponse>>($"api/metodospagos");

    }
}
