using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Pagos
{
    public record UpdatePagoRequest(int SocioId, DateTime FechaPago, decimal Importe, int MetodoPagoId, DateTime? CubreDesde, DateTime? CubreHasta);
}
