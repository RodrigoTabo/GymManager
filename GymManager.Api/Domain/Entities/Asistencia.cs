using GymManager.Shared.Enums;

namespace GymManager.Api.Domain.Entities
{
    public class Asistencia : ICurrentSucursalService
    {
        public int Id { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int SocioId { get; set; }
        public Socio Socio { get; set; } = default!;

        public Guid? SucursalId { get; set; }
    }
}
