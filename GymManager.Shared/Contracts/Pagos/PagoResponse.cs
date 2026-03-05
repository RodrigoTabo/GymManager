using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Pagos
{
    public record PagoResponse(int Id, int SocioId, string Socio, DateTime FechaPago, decimal Importe, int MetodoPagoId, string MetodoPago, DateTime? CubreDesde, DateTime? CubreHasta);
}
