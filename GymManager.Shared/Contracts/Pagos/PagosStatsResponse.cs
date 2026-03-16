using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Pagos
{
    public class PagosStatsResponse
    {
        public int PagosMensualesCount { get; set; }
        public int PagosDiariosCount { get; set; }
        public decimal TotalPagoMensual { get; set; }
        public decimal TotalPagoDiario { get; set; }
        public int PagosDiariosEnEfectivo { get; set; }
        public decimal TotalDiariosPagoEfectivo { get; set; }
        public int PagosDiariosEnTransferencia { get; set; }
        public decimal TotalDiariosPagoTransferencia { get; set; }

        public List<PagoMesSerie> TotalPorMes { get; set; } = new();
    }

    public record PagoMesSerie(string Mes, decimal Total, int Cantidad, decimal TotalEfectivo, decimal TotalTransferencia);
}
