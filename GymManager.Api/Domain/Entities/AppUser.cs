using Microsoft.AspNetCore.Identity;

namespace GymManager.Api.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string Nombre { get; set; } = default!;
        public bool Activo { get; set; } = true;

        public ICollection<UsuarioSucursal> UsuarioSucursales { get; set; } = new List<UsuarioSucursal>();
    }
}
