using System.Net.Http.Json;

namespace GymManager.Client.ApiClients.Common
{
    public static class HttpClientExtension
    {
        //HttpClient extensions: “evita duplicar parseo/errores” Grabate esto wachin, lo estas haciendo espectacular.
        public static async Task<T> GetJsonOrThrowAsync<T>(this HttpClient http, string url)
        {
            var resp = await http.GetAsync(url);

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadFromJsonAsync<T>();
                if (data is null)
                    throw new HttpApiException("La API devolvió una respuesta vacía.");

                return data;
            }

            throw await HttpApiException.FromHttpResponse(resp);
        }

        public static async Task<TResp> PostJsonOrThrowAsync<TReq, TResp>(this HttpClient http, string url, TReq body)
        {
            var resp = await http.PostAsJsonAsync(url, body);

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadFromJsonAsync<TResp>();
                if (data is null)
                    throw new HttpApiException("La API devolvió una respuesta vacía.");

                return data;
            }

            throw await HttpApiException.FromHttpResponse(resp);
        }

        public static async Task PutJsonOrThrowAsync<TReq>(this HttpClient http, string url, TReq body)
        {
            var resp = await http.PutAsJsonAsync(url, body);

            if (resp.IsSuccessStatusCode)
                return;

            throw await HttpApiException.FromHttpResponse(resp);
        }

        public static async Task<TResp> PutJsonOrThrowAsync<TReq, TResp>(this HttpClient http, string url, TReq body)
        {
            var resp = await http.PutAsJsonAsync(url, body);

            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadFromJsonAsync<TResp>();
                if (data is null)
                    throw new HttpApiException("La API devolvió una respuesta vacía.");

                return data;
            }

            throw await HttpApiException.FromHttpResponse(resp);
        }

        public static async Task DeleteOrThrowAsync(this HttpClient http, string url)
        {
            var resp = await http.DeleteAsync(url);

            if (resp.IsSuccessStatusCode)
                return;

            throw await HttpApiException.FromHttpResponse(resp);
        }

        public static async Task PatchOrThrowAsync(this HttpClient http, string url)
        {
            var resp = await http.PatchAsync(url, null);

            if (resp.IsSuccessStatusCode)
                return;

            throw await HttpApiException.FromHttpResponse(resp);
        }

    }
}