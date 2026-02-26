namespace GymManager.Api.Domain.Entities
{
    public class Plan
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? DuracionDias { get; set; }
        public decimal Precio { get; set; }
        public List<Socio> Socios { get; set; } = new();

    }
}
