using GymManager.Shared.Contracts.Pagos;

namespace GymManager.Shared.Contracts.General
{
    public class GeneralResponse
    {
        public int AsistidosHoyCount { get; set; }
        public int AsistidosMesCount { get; set; }
        public int PagosMensualesCount { get; set; }
        public int PagosDiariosCount { get; set; }
        public decimal TotalPagoMensual { get; set; }
        public decimal TotalPagoDiario { get; set; }
        public int CantidadIntentosHoyCount { get; set; }
        public int CantidadIntentosMesCount { get; set; }
        public List<VencidoResponse> TopVencenHoy { get; set; } = new();
        public List<VencidoResponse> TopVencenMes { get; set; } = new();
    }
}
