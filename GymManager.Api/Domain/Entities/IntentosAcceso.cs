using GymManager.Shared.Enums;

namespace GymManager.Api.Domain.Entities
{
    public class IntentosAcceso : ICurrentSucursalService
    {
        public int Id { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string DniIngresado { get; set; } = default!;

        public int? SocioId { get; set; }
        public Socio? Socio { get; set; }
        public ResultadoAcceso Resultado { get; set; }
        public MotivoAcceso Motivo { get; set; }

        public Guid? SucursalId { get; set; }

    }
}
