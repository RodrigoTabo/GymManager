using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Socios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sucursales/{sucursalId:guid}/socios")]
    [Produces("application/json")]
    public class SocioController(ISocioService socioService, ISocioStatsService socioStatsService) : ControllerBase
    {
        private readonly ISocioService _socioService = socioService;
        private readonly ISocioStatsService _socioStatsService = socioStatsService;

        /// <summary>
        /// Lista todos los socios
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<SocioResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SocioResponse>>> Get([FromRoute] Guid sucursalId, [FromQuery] SocioQuery query)
            => Ok(await _socioService.ListarAsync(sucursalId, query));


        [HttpGet("stats")]
        [ProducesResponseType(typeof(SociosStatsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<SociosStatsResponse>> GetStats([FromRoute] Guid sucursalId)
            => Ok(await _socioStatsService.GetStatsAsync(sucursalId));

        /// <summary>
        /// Crea un nuevo socio.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromRoute] Guid sucursalId, [FromBody] CreateSocioRequest request)
        {
            var id = await _socioService.CrearAsync(sucursalId, request);
            return Created($"api/sucursales/{sucursalId}/socios/{id}", new { id });
        }

        /// <summary>
        /// Actualizamos un socio.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put([FromRoute] Guid sucursalId,[FromRoute] int id, [FromBody] UpdateSocioRequest request)
        {
            await _socioService.UpdateAsync(sucursalId ,id, request);
            return NoContent();
        }

        /// <summary>
        /// Traemos socio por Id
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SocioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SocioResponse>> GetByIdAsync([FromRoute] Guid sucursalId,[FromRoute] int id)
            => Ok(await _socioService.GetByIdAsync(sucursalId, id));

        /// <summary>
        /// Eliminamos Socio Logico.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync([FromRoute] Guid sucursalId,[FromRoute]int id)
        {
            await _socioService.SoftDeleteAsync(sucursalId, id);
            return NoContent();
        }

    }
}
