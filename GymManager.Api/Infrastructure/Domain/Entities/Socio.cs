namespace GymManager.Api.Infrastructure.Domain.Entities
{
    public class Socio
    {
        public int Id { get; set; }
        public string DNI { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }
        public int PlanId { get; set; }
        public Plan Plan { get; set; }
        public int? DocumentoId { get; set; }
        public DocumentoSocio? Documento { get; set; }
        public List<Asistencia>? Asistencias { get; set; } = new();
        public List<Pago>? Pagos { get; set; } = new();
        public DateTime? EliminadoEn { get; set; } 
    }
}
