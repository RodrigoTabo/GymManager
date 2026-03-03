using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.MetodoPago;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/metodospagos")]
    [Produces("application/json")]
    public class MetodoPagoController(IMetodoPago metodoPago) : ControllerBase
    {
        private readonly IMetodoPago _MetodoPago = metodoPago;



        /// <summary>
        /// Creamos un Metodo de pago
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CrearAsync([FromBody]CreateMetodoPagoRequest request)
        {
            var id = await _MetodoPago.CrearAsync(request);
            return Created($"/api/metodospagos/{id}", new { id });
        }


    }
}
