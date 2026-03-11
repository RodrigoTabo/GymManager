using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Planes;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class PlanStatsService(GymManagerDbContext context,
        ICurrentSucursalService currentSucursalService,
        ICurrentUserService currentUserService) : IPlanStatsService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentSucursalService _currentSucursalService = currentSucursalService;
        private readonly ICurrentUserService _currentUserService = currentUserService;


        public async Task<StatsPlanRequest> GetStatsAsync(Guid sucursalid)
        {
            var userId = _currentUserService.UserId
              ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

            var sucursalId = _currentSucursalService.SucursalId
                ?? throw new UnauthorizedAccessException("Sucursal no informada.");

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            var autorizado = await _context.UsuarioSucursales
                .AnyAsync(x => x.UsuarioId == userId && x.SucursalId == sucursalId);

            if (!autorizado)
                throw new UnauthorizedAccessException("No tenés acceso a esta sucursal.");

            ///Contamos la cantidad de Planes activos
            var cantidadPlanesActivos = await _context.Planes.Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId).CountAsync();

            var socios = _context.Socios.Where(s => s.EliminadoEn == null && s.SucursalId == sucursalId);

            var top3 = socios
                .GroupBy(x => new { x.PlanId, x.Plan.Nombre, x.Plan.Precio })
                .Select(g => new PlanContadorResponse
                {
                    PlanId = g.Key.PlanId,
                    NombrePlan = g.Key.Nombre,
                    Precio = g.Key.Precio,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(3)
                .ToList();


            var stats = new StatsPlanRequest
            {
                PlanActivos = cantidadPlanesActivos,
                TopPlanes = top3
            };

            return stats;

        }
    }
}
