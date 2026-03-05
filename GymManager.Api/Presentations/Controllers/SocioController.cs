using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Planes;
using GymManager.Shared.Contracts.Socios;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/socios")]
    [Produces("application/json")]
    public class SocioController(ISocioService SocioService) : ControllerBase
    {
        private readonly ISocioService _SocioService = SocioService;

        /// <summary>
        /// Lista todos los socios
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<SocioResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SocioResponse>>> Get([FromQuery] SocioQuery query)
            => Ok(await _SocioService.ListarAsync(query));


        [HttpGet("stats")]
        [ProducesResponseType(typeof(List<SocioResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<SociosStatsResponse>> GetStats()
            => Ok(await _SocioService.GetStatsAsync());

        /// <summary>
        /// Crea un nuevo socio.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromBody] CreateSocioRequest request)
        {
            var id = await _SocioService.CrearAsync(request);
            return Created($"/api/socios/{id}", new { id });
        }

        /// <summary>
        /// Actualiazmos un socio.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put(int id, [FromBody] UpdateSocioRequest request)
        {
            await _SocioService.UpdateAsync(id, request);
            return NoContent();
        }

        /// <summary>
        /// Traemos socio por Id
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SocioResponse>> GetByIdAsync(int id)
            => Ok(await _SocioService.GetByIdAsync(id));

        /// <summary>
        /// Eliminamos Socio Logico.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync(int id)
        {
            await _SocioService.SoftDeleteAsync(id);
            return NoContent();
        }

    }
}
