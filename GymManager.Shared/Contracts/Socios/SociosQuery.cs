using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.Socios
{
    public record SocioQuery
    {
        public string BuscarPor { get; set; } = "NombreCompleto";
        public string Texto { get; set; } = "";
        public bool Inactivo { get; set; } = false;
    }
}
