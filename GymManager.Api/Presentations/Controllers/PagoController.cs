using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sucursales/{sucursalId:guid}/pagos")]
    [Produces("application/json")]
    public class PagoController(IPagoService pagoService, IPagoStatsService pagoStatsService) : ControllerBase
    {
        private readonly IPagoService _pagoService = pagoService;
        private readonly IPagoStatsService _pagoStatsService = pagoStatsService;

        /// <summary>
        /// Listamos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<PagoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PagoResponse>>> Get([FromRoute] Guid sucursalId)
            => Ok(await _pagoService.ListarAsync(sucursalId));

        /// <summary>
        /// Creamos
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromRoute] Guid sucursalId,[FromBody] CreatePagoRequest request)
        {
            var id = await _pagoService.CrearAsync(sucursalId, request);
            return Created($"api/sucursales/{sucursalId}/pagos/{id}", new { id });
        }

        /// <summary>
        /// Updateamos
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put([FromRoute] Guid sucursalId,[FromBody] UpdatePagoRequest request,[FromRoute] int id)
        {
            await _pagoService.UpdateAsync(sucursalId ,request, id);
            return NoContent();
        }

        /// <summary>
        /// Eliminacion logica
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync([FromRoute] Guid sucursalId,[FromRoute] int id)
        {
            await _pagoService.SoftDeleteAsync(sucursalId, id);
            return NoContent();
        }

        /// <summary>
        /// Stats
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(PagosStatsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagosStatsResponse>> GetStatsAsync([FromRoute] Guid sucursalId)
            => Ok(await _pagoStatsService.GetStatsAsync(sucursalId));

    }
}
