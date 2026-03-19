using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    List<Guid> Sucursales { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
                return null;

            // Obtener el claim 'sub' del JWT
            var sub = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public List<Guid> Sucursales
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
                return new List<Guid>();

            // Buscamos el claim "Sucursales"
            var claimValue = user.FindFirst("Sucursales")?.Value;

            if (string.IsNullOrWhiteSpace(claimValue))
                return new List<Guid>();

            try
            {
                return claimValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => Guid.Parse(s.Trim())) // Trim por las dudas
                                 .ToList();
            }
            catch
            {
                return new List<Guid>();
            }
        }
    }
}