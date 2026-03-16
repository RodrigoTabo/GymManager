using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Pagos
{
    public class VencidoResponse
    {
        public string NombreCompleto { get; set; }
        public string Plan { get; set; }
        public DateTime VenceEn { get; set; }
        public decimal Importe { get; set; }
        public uint Telefono { get; set; }

    }
}
