namespace GymManager.Api.Infrastructure.Domain.Entities
{
    public class DocumentoSocio
    {
        public int Id { get; set; }
        public string? Tipo { get; set; }
        public string UrlArchivo { get; set; }
        public DateTime? FechaSubida { get; set; }
        public int SocioId { get; set; }
        public Socio Socio { get; set; }
    }
}
