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
            => Ok(await _generalService.GetStatsGeneralAsync());
    }
}
