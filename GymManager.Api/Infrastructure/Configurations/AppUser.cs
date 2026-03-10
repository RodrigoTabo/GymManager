using Microsoft.AspNetCore.Identity;

namespace GymManager.Api.Infrastructure.Configurations
{
    public class AppUser : IdentityUser<Guid>
    {
        public string Nombre { get; set; } = default!;
        public bool Activo { get; set; } = true;
    }
}
