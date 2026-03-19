using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Planes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/planes")]
    [Produces("application/json")]
    public class PlanController(IPlanService planService, IPlanStatsService planStatsService) : ControllerBase
    {

        private readonly IPlanService _planService = planService;
        private readonly IPlanStatsService _planStatsService = planStatsService;

        /// <summary>
        /// Lista todos los planes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(List<PlanResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PlanResponse>>> Get()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _planService.ListarAsync(sucursalId));
        }

        /// <summary>
        /// Crea un nuevo plan.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromBody] CreatePlanRequest request)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            var id = await _planService.CrearAsync(sucursalId, request);
            return Created($"/api/sucursales/{sucursalId}/planes/{id}", new { id });
        }

        /// <summary>
        /// Actualiza un plan existente
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] UpdatePlanRequest request)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            await _planService.UpdateAsync(sucursalId, id, request);
            return NoContent();
        }

        /// <summary>
        /// Traemos socio por Id
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlanResponse>> GetByIdAsync([FromRoute] int id)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _planService.GetByIdAsync(sucursalId, id));
        }

        /// <summary>
        /// Eliminamos logico
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync([FromRoute] int id)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            await _planService.SoftDeleteAsync(sucursalId, id);
            return NoContent();
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(StatsPlanRequest), StatusCodes.Status200OK)]
        public async Task<ActionResult<StatsPlanRequest>> GetStatsAsync()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _planStatsService.GetStatsAsync(sucursalId));
        }

    }
}
