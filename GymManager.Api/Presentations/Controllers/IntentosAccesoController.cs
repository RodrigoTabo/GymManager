using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.IntentosAcceso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/intentos-acceso")]
    [Produces("application/json")]
    public class IntentosAccesoController(IIntentosAccesoService intentosAcceso) : ControllerBase
    {
        private readonly IIntentosAccesoService _intentosAccesoService = intentosAcceso;


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<IntentosAccesoResponse>>> Listar([FromQuery] IntentosAccesoFiltro filtro)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            var result = await _intentosAccesoService.ListarAsync(sucursalId, filtro);
            return Ok(result);
        }

    }
}