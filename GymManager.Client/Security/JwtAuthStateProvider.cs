using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace GymManager.Web.Security;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStorageService _tokenStorage;

    public JwtAuthStateProvider(TokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(anonymous);
        }

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "UsuarioLogueado")
        ], "Bearer");

        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await _tokenStorage.SetTokenAsync(token);

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "UsuarioLogueado")
        ], "Bearer");

        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await _tokenStorage.RemoveTokenAsync();

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(anonymous)));
    }
}