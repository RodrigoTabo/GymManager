using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Socios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/socios")]
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
        public async Task<ActionResult<List<SocioResponse>>> Get([FromQuery] SocioQuery query)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _socioService.ListarAsync(sucursalId, query));
        }


        [HttpGet("stats")]
        [ProducesResponseType(typeof(SociosStatsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<SociosStatsResponse>> GetStats()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _socioStatsService.GetStatsAsync(sucursalId));
        }

        /// <summary>
        /// Crea un nuevo socio.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromBody] CreateSocioRequest request)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            var id = await _socioService.CrearAsync(sucursalId, request);
            return Created($"api/socios/{id}", new { id });
        }

        /// <summary>
        /// Actualizamos un socio.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] UpdateSocioRequest request)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            await _socioService.UpdateAsync(sucursalId ,id, request);
            return NoContent();
        }

        /// <summary>
        /// Traemos socio por Id
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SocioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SocioResponse>> GetByIdAsync([FromRoute] int id)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _socioService.GetByIdAsync(sucursalId, id));
        }

        /// <summary>
        /// Eliminamos Socio Logico.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync([FromRoute]int id)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            await _socioService.SoftDeleteAsync(sucursalId, id);
            return NoContent();
        }

    }
}
