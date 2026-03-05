using GymManager.Api.Application.Interfaces;
using GymManager.Api.Application.Services;
using GymManager.Shared.Contracts.Pagos;
using GymManager.Shared.Contracts.Socios;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/pagos")]
    [Produces("application/json")]
    public class PagoController(IPagoService pagoService) : ControllerBase
    {
        private readonly IPagoService _pagoService = pagoService;


        /// <summary>
        /// Listamos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<PagoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PagoResponse>>> Get()
            => Ok(await _pagoService.ListarAsync());

        /// <summary>
        /// Creamos
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post(CreatePagoRequest request)
        {
            var id = await _pagoService.CrearAsync(request);
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
        public async Task<ActionResult> Put(UpdatePagoRequest request, int id)
        {
            await _pagoService.UpdateAsync(request, id);
            return NoContent();
        }

        /// <summary>
        /// Eliminacion logica
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> SoftDeleteAsync(int id)
        {
            await _pagoService.SoftDeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Stats
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(List<SocioResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagosStatsResponse>> GetStatsAsync()
            => Ok(await _pagoService.GetPagosStatsAsync());

    }
}
