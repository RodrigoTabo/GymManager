using GymManager.Shared.Contracts.Pagos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Socios
{
    public class SociosStatsResponse
    {
        public int ActivosCount { get; set; }
        public int InactivosCount { get; set; }
        public int AltasMesCount { get; set; }
        public decimal CobroMestotal { get; set; }
        public List<PagoResponse> UltimosPagos { get; set; } = new();
        public List<PagoResponse> Morosos { get; set; } = new();
        public List<string> Meses { get; set; } = new();
        public List<int> AltasPorMes { get; set; }
        public List<int> BajasPorMes { get; set; }

    }
}
