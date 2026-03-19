using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/pagos")]
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
        public async Task<ActionResult<List<PagoResponse>>> Get()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _pagoService.ListarAsync(sucursalId));
        }

        /// <summary>
        /// Creamos
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post([FromBody] CreatePagoRequest request)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);

            var id = await _pagoService.CrearAsync(sucursalId, request);
            return Created($"api/pagos/{id}", new { id });
        }

        /// <summary>
        /// Updateamos
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Put([FromBody] UpdatePagoRequest request, [FromRoute] int id)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            await _pagoService.UpdateAsync(sucursalId, request, id);
            return NoContent();
        }

        /// <summary>
        /// Eliminacion logica
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync([FromRoute] int id)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            await _pagoService.SoftDeleteAsync(sucursalId, id);
            return NoContent();
        }

        /// <summary>
        /// Stats
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(PagosStatsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagosStatsResponse>> GetStatsAsync()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _pagoStatsService.GetStatsAsync(sucursalId));
        }

        /// <summary>
        /// Vencidos
        /// </summary>
        [HttpGet("vencidos")]
        [ProducesResponseType(typeof(List<VencidoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<VencidoResponse>>> GetVencidosAsync()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _pagoService.GetVencidosAsync(sucursalId));
        }

        /// <summary>
        /// Stats Vencidos
        /// </summary>
        [HttpGet("vencidos/stats")]
        [ProducesResponseType(typeof(VencimientoStatsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<VencimientoStatsResponse>> GetVencidoStatsAsync()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            return Ok(await _pagoStatsService.GetVencidosStatsAsync(sucursalId));
        }

    }
}
