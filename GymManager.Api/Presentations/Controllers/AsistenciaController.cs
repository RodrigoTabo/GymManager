using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Asistencias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sucursales/{sucursalId:guid}/asistencias")]
    [Produces("application/json")]
    public class AsistenciaController(IAsistenciaService asistenciaService) : ControllerBase
    {
        private readonly IAsistenciaService _asistenciaService = asistenciaService;


        [HttpGet]
        public async Task<ActionResult<List<AsistenciaResponse>>> Get([FromRoute] Guid sucursalId, [FromQuery] AsistenciaFiltro filtro)
            => Ok(await _asistenciaService.ListarAsync(sucursalId, filtro));


        [HttpPost("marcar")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<MarcarAsistenciaResponse>> Post([FromRoute] Guid sucursalId, [FromBody] MarcarAsistenciaRequest request)
        {
            var resp = await _asistenciaService.MarcarPorDniAsync(sucursalId, request.DNI);

            return Created($"api/sucursales/{sucursalId}/asistencias/{resp.Id}", resp);
        }

    }
}
