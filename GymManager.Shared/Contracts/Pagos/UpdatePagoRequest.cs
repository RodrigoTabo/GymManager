using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Pagos
{
    public record UpdatePagoRequest(int MetodoPagoId, DateTime FechaPago);
}
