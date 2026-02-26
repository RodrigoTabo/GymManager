using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Asistencias
{
    public record AsistenciaResponse(int Id, int SocioId, DateTime FechaHora, string Resultado, string Motivo, int RegistradoPor);
}
