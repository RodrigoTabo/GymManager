using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Socios
{
    public record UpdateSocioRequest(string DNI, string Nombre, string Apellido, uint Telefono, DateTime? FechaNacimiento, int PlanId, int? documentoId);
}
