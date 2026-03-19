using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Sucursal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GymManager.Api.Presentations.Controllers
{
    [Authorize] // <--- IMPORTANTE: Solo usuarios con el token de login
    [ApiController]
    [Route("api/sucursales")]
    public class SucursalController(ISucursalService sucursalService) : ControllerBase
    {
        private readonly ISucursalService _sucursalService = sucursalService;

        [HttpGet] // <--- Endpoint específico
        public async Task<ActionResult<List<SucursalResponse>>> Get()
        {
            // Extraemos el ID del usuario del token JWT que envió el Front
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Llamamos al servicio pasando el userId para filtrar
            var sucursales = await _sucursalService.GetSucursalAsync(userId);

            return Ok(sucursales);
        }
    }
}
