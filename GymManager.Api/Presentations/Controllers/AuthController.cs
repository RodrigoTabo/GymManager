using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;
    private readonly GymManagerDbContext _context;

    public AuthController(UserManager<AppUser> userManager, IConfiguration config, GymManagerDbContext context)
    {
        _userManager = userManager;
        _config = config;
        _context = context;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Usuario o contraseña incorrectos");

        // Generamos token inicial (traerá la lista de sucursales pero sin una seleccionada)
        var token = await GenerateJwtToken(user, null);

        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpPost("select-branch")]
    public async Task<IActionResult> SelectBranch([FromBody] string sucursalId)
    {
        // Buscamos el ID del usuario del token actual
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // Validamos acceso en la tabla puente
        var tieneAcceso = await _context.UsuarioSucursales
            .AnyAsync(us => us.UsuarioId.ToString() == userId && us.SucursalId.ToString() == sucursalId);

        if (!tieneAcceso) return Forbid("No tienes acceso a esta sucursal");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // Generamos el nuevo token incluyendo el SucursalId seleccionado
        var nuevoToken = await GenerateJwtToken(user, sucursalId);

        return Ok(new { Token = nuevoToken });
    }

    private async Task<string> GenerateJwtToken(AppUser user, string? sucursalIdSeleccionada)
    {
        // 1. Buscamos todas las sucursales del usuario para incluirlas en el claim "Sucursales"
        var sucursalesIds = await _context.UsuarioSucursales
            .Where(us => us.UsuarioId == user.Id)
            .Select(us => us.SucursalId.ToString())
            .ToListAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Guardamos la lista separada por comas
            new Claim("Sucursales", string.Join(",", sucursalesIds))
        };

        // 2. Si se seleccionó una sucursal específica, agregamos el claim individual
        if (!string.IsNullOrEmpty(sucursalIdSeleccionada))
        {
            claims.Add(new Claim("SucursalId", sucursalIdSeleccionada));
        }

        // 3. Generación técnica del token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
