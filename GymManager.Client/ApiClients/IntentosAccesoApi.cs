using GymManager.Client.ApiClients.Common;
using GymManager.Shared.Contracts.IntentosAcceso;
using System.Collections;

namespace GymManager.Client.ApiClients
{
    public class IntentosAccesoApi(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<IntentosAccesoResponse>> ListarAsync(IntentosAccesoFiltro filtro)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(filtro.Dni))
                queryParams.Add($"dni={Uri.EscapeDataString(filtro.Dni.Trim())}");

            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                queryParams.Add($"nombre={Uri.EscapeDataString(filtro.Nombre.Trim())}");

            if (filtro.Resultado.HasValue)
                queryParams.Add($"resultado={(int)filtro.Resultado.Value}");

            if (filtro.Motivo.HasValue)
                queryParams.Add($"motivo={(int)filtro.Motivo.Value}");

            if (filtro.Desde.HasValue)
                queryParams.Add($"desde={Uri.EscapeDataString(filtro.Desde.Value.ToString("O"))}");

            if (filtro.Hasta.HasValue)
                queryParams.Add($"hasta={Uri.EscapeDataString(filtro.Hasta.Value.ToString("O"))}");

            var url = "api/intentosaccesos";

            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            return await _httpClient.GetJsonOrThrowAsync<List<IntentosAccesoResponse>>(url);
        }


    }
}
