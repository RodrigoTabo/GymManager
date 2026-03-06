using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.IntentosAcceso;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/IntentosAccesos")]
    [Produces("application/json")]
    public class IntentosAccesoController(IIntentosAccesoService intentosAcceso) : ControllerBase
    {
        private readonly IIntentosAccesoService _intentosAccesoService = intentosAcceso;
        [HttpGet]
        public async Task<ActionResult<List<IntentosAccesoResponse>>> Listar([FromQuery] IntentosAccesoFiltro filtro)
        {
            var result = await _intentosAccesoService.ListarAsync(filtro);
            return Ok(result);
        }

    }
}