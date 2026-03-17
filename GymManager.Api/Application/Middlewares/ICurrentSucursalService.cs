using GymManager.Api.Domain.Entities;

public class CurrentSucursalService(IHttpContextAccessor httpContextAccessor) : ICurrentSucursalService
{

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid? SucursalId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
                return null;

            if (!httpContext.Request.RouteValues.TryGetValue("sucursalId", out var value))
                return null;

            return Guid.TryParse(value?.ToString(), out var id) ? id : null;
        }
    }
}