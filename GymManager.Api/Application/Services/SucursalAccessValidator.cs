using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class SucursalAccessValidator : ISucursalAccessValidator
    {
        private readonly GymManagerDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public SucursalAccessValidator(GymManagerDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> ValidarYObtenerSucursalAsync(Guid sucursalIdParam)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

            var sucursalesPermitidas = _currentUserService.Sucursales;

            if (!sucursalesPermitidas.Contains(sucursalIdParam))
                throw new UnauthorizedAccessException("No tenés acceso a esta sucursal.");

            var autorizado = await _context.UsuarioSucursales
                .AnyAsync(x => x.UsuarioId == userId &&
                               x.SucursalId == sucursalIdParam &&
                               x.Sucursal.Activa);

            if (!autorizado)
                throw new UnauthorizedAccessException("Sucursal inválida o inactiva.");

            return sucursalIdParam;
        }
    }
}
