namespace GymManager.Api.Domain.Entities
{
    public class Asistencia
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Resultado { get; set; }
        public string? Motivo { get; set; }
        public int SocioId { get; set; }
        public Socio Socio { get; set; }
    }
}
