using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Asistencias
{
    public class AsistenciaResponse
    {
        public int Id { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int SocioId { get; set; }
        public string Socio { get; set; } = default!;
    }
}
