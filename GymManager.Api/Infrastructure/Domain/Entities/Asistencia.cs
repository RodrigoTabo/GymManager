using GymManager.Shared.Enums;

namespace GymManager.Api.Infrastructure.Domain.Entities
{
    public class Asistencia
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public ResultadoAsistencia Resultado { get; set; }
        public MotivoAsistencia Motivo { get; set; }
        public int SocioId { get; set; }
        public Socio Socio { get; set; } = default!;
    }
}
