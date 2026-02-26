using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Socios
{
    public record CreateSocioRequest(int DNI, string Nombre, string Apellido, DateTime Edad, DateTime? FechaAlta, DateTime? FechaBaja, int PlanId);

}
