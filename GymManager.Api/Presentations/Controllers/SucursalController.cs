using GymManager.Api.Application.Interfaces;
using GymManager.Shared.Contracts.Sucursal;
using Microsoft.AspNetCore.Mvc;

namespace GymManager.Api.Presentations.Controllers
{
    [ApiController]
    [Route("api/sucursales")]
    [Produces("application/json")]
    public class SucursalController(ISucursalService sucursalService) : ControllerBase
    {
        private readonly ISucursalService _sucursalService = sucursalService;

        [HttpGet]
        public async Task<ActionResult<List<SucursalResponse>>> Get()
        => Ok(await _sucursalService.GetSucursalAsync());

    }
}
