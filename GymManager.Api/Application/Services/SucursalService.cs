using GymManager.Api.Application.Interfaces;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Sucursal;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class SucursalService(GymManagerDbContext context, ICurrentUserService currentUserService) : ISucursalService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<List<SucursalResponse>> GetSucursalAsync()
        {
            var userId = _currentUserService.UserId;

            if (userId is null)
                throw new UnauthorizedAccessException("Usuario no autenticado.");

            var sucursales = await _context.UsuarioSucursales
                .AsNoTracking()
                .Where(x => x.UsuarioId == userId && x.Sucursal.Activa)
                .Select(x => new SucursalResponse
                {
                    Id = x.SucursalId,
                    Nombre = x.Sucursal.Nombre
                })
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            return sucursales;
        }
    }
}
