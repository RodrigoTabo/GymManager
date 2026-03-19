using GymManager.Api.Application.Interfaces;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Sucursal;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class SucursalService : ISucursalService
    {
        private readonly GymManagerDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SucursalService(GymManagerDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<SucursalResponse>> GetSucursalAsync(string userId)
        {
            var sucursalesPermitidas = _currentUserService.Sucursales;

            var sucursales = await _context.UsuarioSucursales
                .AsNoTracking()
                .Where(x => x.UsuarioId.ToString() == userId &&
                            sucursalesPermitidas.Contains(x.SucursalId) &&
                            x.Sucursal.Activa)
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
