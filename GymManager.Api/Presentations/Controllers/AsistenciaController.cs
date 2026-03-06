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


        [HttpGet]
        public async Task<ActionResult<List<AsistenciaResponse>>> Get([FromQuery] AsistenciaFiltro filtro)
            => Ok(await _asistenciaService.ListarAsync(filtro));


        [HttpPost("marcar")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<MarcarAsistenciaResponse>> Post([FromBody] MarcarAsistenciaRequest request)
        {
            var resp = await _asistenciaService.MarcarPorDniAsync(request.DNI);

            return Created($"/api/asistencias/{resp.Id}", resp);
        }

    }
}
