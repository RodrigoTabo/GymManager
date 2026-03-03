using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/pagos")]
    [Produces("application/json")]
    public class PagoController(IPagoService pagoService) : ControllerBase
    {
        private readonly IPagoService _pagoService = pagoService;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> Post(CreatePagoRequest request)
        {
            var id = await _pagoService.CrearAsync(request);
            return Created($"api/pagos/{id}", new { id });
        }


    }
}
