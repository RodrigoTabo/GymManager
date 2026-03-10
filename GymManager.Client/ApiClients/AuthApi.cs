using System.Net.Http.Json;
using GymManager.Shared.Auth;

namespace GymManager.Web.ApiClients;

public class AuthApi
{
    private readonly HttpClient _httpClient;

    public AuthApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("login?useCookies=false&useSessionCookies=false", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (result is null)
            throw new Exception("No se pudo leer la respuesta de login.");

        return result;
    }
}