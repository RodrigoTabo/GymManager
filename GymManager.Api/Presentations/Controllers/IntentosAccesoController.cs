using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.IntentosAcceso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sucursales/{sucursalId:guid}/intentos-acceso")]
    [Produces("application/json")]
    public class IntentosAccesoController(IIntentosAccesoService intentosAcceso) : ControllerBase
    {
        private readonly IIntentosAccesoService _intentosAccesoService = intentosAcceso;


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<IntentosAccesoResponse>>> Listar([FromRoute] Guid sucursalId, [FromQuery] IntentosAccesoFiltro filtro)
        {
            var result = await _intentosAccesoService.ListarAsync(sucursalId, filtro);
            return Ok(result);
        }

    }
}