namespace GymManager.Api.Domain.Entities
{
    public class Sucursal
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = default!;
        public bool Activa { get; set; } = true;

    }
}
