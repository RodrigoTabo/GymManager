using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Planes;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/planes")]
    [Produces("application/json")]
    public class PlanController(IPlanService planService) : ControllerBase
    {
        private readonly IPlanService _planService = planService;

        /// <summary>
        /// Lista todos los planes
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(List<PlanResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PlanResponse>>> Get()
            => Ok(await _planService.ListarAsync());

        /// <summary>
        /// Crea un nuevo plan.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromBody] CreatePlanRequest request)
        {
            var id = await _planService.CrearAsync(request);
            return Created($"/api/planes/{id}", new { id });
        }

        /// <summary>
        /// Actualiza un plan existente
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put(int id, [FromBody] UpdatePlanRequest request)
        {
            await _planService.UpdateAsync(id, request);
            return NoContent();
        }

        /// <summary>
        /// Traemos socio por Id
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PlanResponse>> GetByIdAsync(int id)
        => Ok(await _planService.GetByIdAsync(id));

        /// <summary>
        /// Eliminamos logico
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync(int id)
        {
            await _planService.SoftDeleteAsync(id);
            return NoContent();
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(StatsPlanRequest), StatusCodes.Status200OK)]
        public async Task<ActionResult<StatsPlanRequest>> GetStatsAsync()
            => Ok(await _planService.GetStatsAsync());


    }
}
