using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Pagos
{
    public class VencimientoStatsResponse
    {
        public int VencenHoy { get; set; }
        public decimal TotalCobrarHoy { get; set; }
        public int VencenEstaSemana { get; set; }
        public decimal TotalCobrarSemana { get; set; }
        public int Vencidos { get; set; }
    }
}
