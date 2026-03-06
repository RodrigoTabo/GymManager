using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.Asistencias;

namespace GymManager.Client.ApiClients
{
    public class AsistenciaApi (HttpClient HttpClient)
    {

        private readonly HttpClient _httpClient = HttpClient;

        public async Task<List<AsistenciaResponse>> ListarAsync(AsistenciaFiltro filtro)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(filtro.Dni))
                queryParams.Add($"dni={Uri.EscapeDataString(filtro.Dni.Trim())}");

            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                queryParams.Add($"nombre={Uri.EscapeDataString(filtro.Nombre.Trim())}");

            if (filtro.Desde.HasValue)
                queryParams.Add($"desde={Uri.EscapeDataString(filtro.Desde.Value.ToString("O"))}");

            if (filtro.Hasta.HasValue)
                queryParams.Add($"hasta={Uri.EscapeDataString(filtro.Hasta.Value.ToString("O"))}");

            var url = "api/asistencias";

            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            return await _httpClient.GetJsonOrThrowAsync<List<AsistenciaResponse>>(url);

        }
        public async Task<MarcarAsistenciaResponse> MarcarPorDniAsync(MarcarAsistenciaRequest request)
        {
            var resp = await _httpClient.PostJsonOrThrowAsync<MarcarAsistenciaRequest, MarcarAsistenciaResponse>(
                "api/asistencias/marcar",
                request);

            return resp;
        }

    }
}
