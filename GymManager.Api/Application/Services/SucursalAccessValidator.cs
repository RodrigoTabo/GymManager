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
        private readonly ICurrentSucursalService _currentSucursalService;

        public SucursalAccessValidator(
            GymManagerDbContext context,
            ICurrentUserService currentUserService,
            ICurrentSucursalService currentSucursalService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _currentSucursalService = currentSucursalService;
        }

        public async Task<Guid> ValidarYObtenerSucursalAsync(Guid sucursalIdParam)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

            var sucursalId = _currentSucursalService.SucursalId
                ?? throw new UnauthorizedAccessException("Sucursal no informada.");

            if (sucursalIdParam != sucursalId)
                throw new UnauthorizedAccessException("Sucursal inválida.");

            var autorizado = await _context.UsuarioSucursales
                .AnyAsync(x => x.UsuarioId == userId && x.SucursalId == sucursalId);

            if (!autorizado)
                throw new UnauthorizedAccessException("No tenés acceso a esta sucursal.");

            return sucursalId;
        }
    }
}
