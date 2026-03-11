namespace GymManager.Api.Domain.Entities
{
    public class DocumentoSocio : ICurrentSucursalService
    {
        public int Id { get; set; }
        public string? Tipo { get; set; }
        public string UrlArchivo { get; set; }
        public DateTime? FechaSubida { get; set; }
        public int SocioId { get; set; }
        public Socio Socio { get; set; }

        public Guid? SucursalId { get; set; }
    }
}
