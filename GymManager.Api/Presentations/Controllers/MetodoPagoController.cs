using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.MetodoPago;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/metodospagos")]
    [Produces("application/json")]
    public class MetodoPagoController(IMetodoPagoService metodoPago) : ControllerBase
    {
        private readonly IMetodoPagoService _metodoPago = metodoPago;



        [HttpGet]
        [ProducesResponseType(typeof(List<MetodoPagoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MetodoPagoResponse>>> Get()
            => Ok(await _metodoPago.ListarAsync());

        /// <summary>
        /// Creamos un Metodo de pago
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CrearAsync([FromBody] CreateMetodoPagoRequest request)
        {
            var id = await _metodoPago.CrearAsync(request);
            return Created($"/api/metodospagos/{id}", new { id });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> UpdateAsync([FromBody] UpdateMetodoPagoRequest request, int id)
        {
            await _metodoPago.UpdateAsync(request, id);
            return NoContent();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync(int id)
        {
            await _metodoPago.SoftDeleteAsync(id);
            return NoContent();
        }


    }
}
