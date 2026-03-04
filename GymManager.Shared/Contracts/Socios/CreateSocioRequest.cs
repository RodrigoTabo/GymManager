using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Socios
{
    public record CreateSocioRequest(string DNI, string Nombre, string Apellido, DateTime? FechaNacimiento, int PlanId, int? documentoId);
}
