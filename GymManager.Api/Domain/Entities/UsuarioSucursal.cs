namespace GymManager.Api.Domain.Entities
{
    public class UsuarioSucursal
    {
        public Guid UsuarioId { get; set; }
        public AppUser Usuario { get; set; } = default!;

        public Guid SucursalId { get; set; }
        public Sucursal Sucursal { get; set; } = default!;

        public bool EsPrincipal { get; set; }

    }
}
