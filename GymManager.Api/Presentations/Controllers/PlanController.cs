using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Planes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sucursales/{sucursalId:guid}/planes")]
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
        public async Task<ActionResult<List<PlanResponse>>> Get([FromRoute] Guid sucursalId)
            => Ok(await _planService.ListarAsync(sucursalId));

        /// <summary>
        /// Crea un nuevo plan.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromRoute] Guid sucursalId, [FromBody] CreatePlanRequest request)
        {
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
        public async Task<ActionResult> Put([FromRoute] Guid sucursalId, [FromRoute] int id, [FromBody] UpdatePlanRequest request)
        {
            await _planService.UpdateAsync(sucursalId, id, request);
            return NoContent();
        }

        /// <summary>
        /// Traemos socio por Id
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlanResponse>> GetByIdAsync([FromRoute] Guid sucursalId,[FromRoute] int id)
        => Ok(await _planService.GetByIdAsync(sucursalId, id));

        /// <summary>
        /// Eliminamos logico
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync([FromRoute] Guid sucursalId,[FromRoute] int id)
        {
            await _planService.SoftDeleteAsync(sucursalId, id);
            return NoContent();
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(StatsPlanRequest), StatusCodes.Status200OK)]
        public async Task<ActionResult<StatsPlanRequest>> GetStatsAsync([FromRoute] Guid sucursalId)
            => Ok(await _planStatsService.GetStatsAsync(sucursalId));

    }
}
