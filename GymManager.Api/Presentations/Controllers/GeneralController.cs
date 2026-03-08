using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.General;
using GymManager.Shared.Contracts.Socios;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/general")]
    [Produces("application/json")]
    public class GeneralController(IGeneralService generalService) : ControllerBase
    {
        private readonly IGeneralService _generalService = generalService;

        [HttpGet]
        [ProducesResponseType(typeof(GeneralResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GeneralResponse>> Get()
            => Ok(await _generalService.GetStatsGeneralAsync());

    }
}
