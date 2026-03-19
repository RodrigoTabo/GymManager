using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Asistencias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/asistencias")]
    [Produces("application/json")]
    public class AsistenciaController(IAsistenciaService asistenciaService) : ControllerBase
    {
        private readonly IAsistenciaService _asistenciaService = asistenciaService;


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AsistenciaResponse>>> Get([FromQuery] AsistenciaFiltro filtro)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);

            return Ok(await _asistenciaService.ListarAsync(sucursalId, filtro));
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<MarcarAsistenciaResponse>> Post([FromBody] MarcarAsistenciaRequest request)
        {
            // Obtenemos el SucursalId del JWT
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            if (sucursalIdClaim == null)
                return Forbid(); // usuario no autorizado si no tiene claim

            var sucursalId = Guid.Parse(sucursalIdClaim);

            var resp = await _asistenciaService.MarcarPorDniAsync(sucursalId, request.DNI);

            return Created($"api/asistencias/{resp.Id}", resp);
        }

    }
}
