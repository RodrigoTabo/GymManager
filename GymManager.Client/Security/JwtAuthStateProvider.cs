using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace GymManager.Web.Security;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStorageService _tokenStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public JwtAuthStateProvider(TokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenStorage.GetTokenAsync();

            // Si no hay token o no parece un JWT (minimo 2 puntos), es anonimo
            if (string.IsNullOrWhiteSpace(token) || !token.Contains("."))
            {
                return new AuthenticationState(_anonymous);
            }

            var user = BuildUserFromToken(token);
            return new AuthenticationState(user);
        }
        catch
        {
            // Si algo falla al leer el token, lo tratamos como no autenticado
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await _tokenStorage.SetTokenAsync(token);
        var user = BuildUserFromToken(token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await _tokenStorage.RemoveTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private ClaimsPrincipal BuildUserFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            // Verificamos si el handler puede leerlo antes de intentar
            if (!handler.CanReadToken(token))
                return _anonymous;

            var jwt = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwt.Claims, "Bearer");

            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return _anonymous;
        }
    }
}