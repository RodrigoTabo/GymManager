using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManager.Shared.Contracts.DocumentoSocio
{
    public record DocumentoSocioResponse(int Id, int SocioId, string Tipo, string Documento, DateTime? FechaDeSubida);

}
