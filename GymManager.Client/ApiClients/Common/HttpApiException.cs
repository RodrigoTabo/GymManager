using System.Net;
using System.Net.Http.Json;

namespace GymManager.Client.ApiClients.Common
{
    public class HttpApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpApiException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpApiException(string? message) : base(message)
        {
        }

        public static async Task<HttpApiException> FromHttpResponse(HttpResponseMessage resp)
        {
            var status = resp.StatusCode;

            try
            {
                var vpd = await resp.Content.ReadFromJsonAsync<ValidationProblemDetailsLite>();
                if (vpd?.Errors is not null && vpd.Errors.Count > 0)
                {
                    var first = vpd.Errors
                        .SelectMany(kv => kv.Value ?? Array.Empty<string>())
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(first))
                        return new HttpApiException(first, status);
                }

                var pd = await resp.Content.ReadFromJsonAsync<ProblemDetailsLite>();
                var msg = pd?.Title ?? "Ocurrió un error.";

                if (!string.IsNullOrWhiteSpace(msg))
                    return new HttpApiException(msg, status);
            }
            catch
            {
                // ignore parse errors
            }

            // 3) fallback
            var text = await resp.Content.ReadAsStringAsync();
            var fallback = string.IsNullOrWhiteSpace(text) ? $"HTTP {(int)status}" : text;
            return new HttpApiException(fallback, status);
        }

        private class ValidationProblemDetailsLite
        {
            public string? Title { get; set; }
            public Dictionary<string, string[]?>? Errors { get; set; }
        }

        private class ProblemDetailsLite
        {
            public string? Title { get; set; }
            public string? Detail { get; set; }
            public int? Status { get; set; }
        }
    }
}

