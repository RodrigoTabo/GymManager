using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Pagos
{
    public record CreatePagoRequest(int SocioId, DateTime FechaPago, float Monto, int MetodoPagoId, DateTime? CubreDesde, DateTime? CubreHasta);
}
