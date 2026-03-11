using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.MetodoPago;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sucursales/{sucursalId:guid}/metodos-pago")]
    [Produces("application/json")]
    public class MetodoPagoController(IMetodoPagoService metodoPago) : ControllerBase
    {
        private readonly IMetodoPagoService _metodoPago = metodoPago;

        [HttpGet]
        [ProducesResponseType(typeof(List<MetodoPagoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MetodoPagoResponse>>> Get([FromRoute] Guid sucursalId)
            => Ok(await _metodoPago.ListarAsync(sucursalId));

        /// <summary>
        /// Creamos un Metodo de pago
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CrearAsync([FromRoute] Guid sucursalId, [FromBody] CreateMetodoPagoRequest request)
        {
            var id = await _metodoPago.CrearAsync(sucursalId, request);
            return Created($"api/sucursales/{sucursalId}/metodos-pago/{id}", new { id });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> UpdateAsync([FromRoute] Guid sucursalId, [FromRoute] int id, [FromBody] UpdateMetodoPagoRequest request)
        {
            await _metodoPago.UpdateAsync(sucursalId, request, id);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync([FromRoute] Guid sucursalId, [FromRoute] int id)
        {
            await _metodoPago.SoftDeleteAsync(sucursalId, id);
            return NoContent();
        }
    }
}
