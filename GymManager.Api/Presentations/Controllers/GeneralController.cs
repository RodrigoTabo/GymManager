using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.General;
using GymManager.Shared.Contracts.Socios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sucursales/{sucursalId:guid}/general")]

    [Produces("application/json")]
    public class GeneralController(IGeneralService generalService) : ControllerBase
    {
        private readonly IGeneralService _generalService = generalService;

        [HttpGet]
        [ProducesResponseType(typeof(GeneralResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GeneralResponse>> Get([FromRoute] Guid sucursalId)
            => Ok(await _generalService.GetStatsGeneralAsync(sucursalId));

    }
}
