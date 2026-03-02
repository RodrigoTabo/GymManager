using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Socios
{
    public record SocioResponse(string DNI, string Nombre, string Apellido, DateTime? FechaNacimiento, DateTime? FechaAlta, DateTime? FechaBaja, int PlanId);
}
