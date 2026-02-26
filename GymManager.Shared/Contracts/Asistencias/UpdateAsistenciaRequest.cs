using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Asistencias
{
    public record UpdateAsistenciaRequest(int Id, int SocioId, DateTime FechaHora, string Resultado, string Motivo, int RegistradoPor);
}
