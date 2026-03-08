using GymManager.Shared.Contracts.Pagos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<PagosVencidos> TopVencenHoy { get; set; } = new();
        public List<PagosVencidos> TopVencenMes { get; set; } = new();
    }


    public class PagosVencidos
    {
        public int PagoId { get; set; }
        public string NombreSocio { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }
}
