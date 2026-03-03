using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Asistencias;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/Asistencias")]
    [Produces("application/json")]
    public class AsistenciaController(IAsistenciaService asistenciaService) : ControllerBase
    {
        private readonly IAsistenciaService _asistenciaService = asistenciaService;

        [HttpPost("marcar")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<MarcarAsistenciaResponse>> Post([FromBody] MarcarAsistenciaRequest request)
        {
            var id = await _asistenciaService.MarcarPorDniAsync(request.DNI);

            return Created($"/api/asistencias/{id}", new { id });
        }


    }
}
