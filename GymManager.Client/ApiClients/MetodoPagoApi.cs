using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.MetodoPago;
using GymManager.Web.Security;

namespace GymManager.Client.ApiClients
{
    public class MetodoPagoApi(ApiHttpClientProvider clientProvider)
    {

        private readonly ApiHttpClientProvider _clientProvider = clientProvider;

        public async Task<List<MetodoPagoResponse>> ListarAsync()
        {
            var client = await _clientProvider.GetClientAsync();
            return await client.GetJsonOrThrowAsync<List<MetodoPagoResponse>>($"api/metodos-pago");
        }

    }
}
