namespace GymManager.Api.Domain.Entities
{
    public class MetodoPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public List<Pago> Pagos { get; set; } = new();
        public DateTime? EliminadoEn { get; set; }
    }
}
