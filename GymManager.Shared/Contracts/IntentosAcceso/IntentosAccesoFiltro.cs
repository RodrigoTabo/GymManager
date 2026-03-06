using GymManager.Shared.Enums;

namespace GymManager.Shared.Contracts.IntentosAcceso
{
    public class IntentosAccesoFiltro
    {
        public string? Dni { get; set; }
        public string? Nombre { get; set; }
        public ResultadoAcceso? Resultado { get; set; }
        public MotivoAcceso? Motivo { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
    }
}
