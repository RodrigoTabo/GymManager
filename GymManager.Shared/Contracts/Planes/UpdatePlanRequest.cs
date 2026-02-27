using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Planes
{
    public record UpdatePlanRequest(int Id, string Nombre, int DuracionDias, decimal Precio);
}
