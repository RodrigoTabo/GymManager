using GymManager.Shared.Enums;

namespace GymManager.Shared.Contracts.IntentosAcceso
{
    public class IntentosAccesoResponse
    {
        public DateTime FechaRegistro { get; set; }
        public string DniIngresado { get; set; } = default!;
        public int? SocioId { get; set; }
        public string? Socio { get; set; }
        public string Resultado { get; set; }
        public string Motivo { get; set; }
    }
}
