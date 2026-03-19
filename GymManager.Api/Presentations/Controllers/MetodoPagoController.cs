using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.MetodoPago;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/metodos-pago")]
    [Produces("application/json")]
    public class MetodoPagoController(IMetodoPagoService metodoPago) : ControllerBase
    {
        private readonly IMetodoPagoService _metodoPago = metodoPago;

        [HttpGet]
        [ProducesResponseType(typeof(List<MetodoPagoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MetodoPagoResponse>>> Get()
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);

            return Ok(await _metodoPago.ListarAsync(sucursalId));
        }

        /// <summary>
        /// Creamos un Metodo de pago
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CrearAsync([FromBody] CreateMetodoPagoRequest request)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);

            var id = await _metodoPago.CrearAsync(sucursalId, request);
            return Created($"api/metodos-pago/{id}", new { id });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> UpdateAsync([FromRoute] int id, [FromBody] UpdateMetodoPagoRequest request)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            await _metodoPago.UpdateAsync(sucursalId, request, id);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync([FromRoute] int id)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);
            await _metodoPago.SoftDeleteAsync(sucursalId, id);
            return NoContent();
        }
    }
}
