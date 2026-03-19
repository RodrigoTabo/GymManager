using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.General;
using GymManager.Shared.Contracts.Socios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/general")]
    [Produces("application/json")]
    public class GeneralController : ControllerBase
    {
        private readonly IGeneralService _generalService;

        public GeneralController(IGeneralService generalService)
        {
            _generalService = generalService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GeneralResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GeneralResponse>> Get()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);

            var stats = await _generalService.GetStatsGeneralAsync(sucursalId);
            return Ok(stats);
        }
    }
}
