namespace GymManager.Api.Infrastructure.Domain.Entities
{
    public class MetodoPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public List<Pago> Pagos { get; set; } = new();
    }
}
