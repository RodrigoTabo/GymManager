using System.Net.Http.Headers;

namespace GymManager.Web.Security
{
    public class ApiHttpClientProvider
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStorageService _tokenStorage;

        public ApiHttpClientProvider(HttpClient httpClient, TokenStorageService tokenStorage)
        {
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
        }

        public async Task<HttpClient> GetClientAsync()
        {
            var token = await _tokenStorage.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            else
                _httpClient.DefaultRequestHeaders.Authorization = null;

            return _httpClient;
        }

        public void ClearAuthorization()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}