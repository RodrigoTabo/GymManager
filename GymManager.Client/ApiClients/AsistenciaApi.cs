using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Asistencias;

namespace GymManager.Client.ApiClients
{
    public class AsistenciaApi (HttpClient HttpClient)
    {

        private readonly HttpClient _httpClient = HttpClient;


        public async Task<MarcarAsistenciaResponse> MarcarPorDniAsync(MarcarAsistenciaRequest request)
        {
            var resp = await _httpClient.PostJsonOrThrowAsync<MarcarAsistenciaRequest, MarcarAsistenciaResponse>(
                "api/asistencias/marcar",
                request);

            return resp;
        }

    }
}
