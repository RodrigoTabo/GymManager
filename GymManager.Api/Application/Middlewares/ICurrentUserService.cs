using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? SucursalId { get; }
    Guid SucursalIdOrThrow { get; }
    List <Guid> Sucursales { get; }
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

    public Guid? SucursalId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity!.IsAuthenticated)
                return null;

            var claim = user.FindFirst("SucursalId")?.Value;

            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public Guid SucursalIdOrThrow
    {
        get
        {
            var id = SucursalId;

            if (id is null)
                throw new UnauthorizedAccessException("Sucursal no definida en el token");

            return id.Value;
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