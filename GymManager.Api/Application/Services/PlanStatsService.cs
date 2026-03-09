using GymManager.Api.Application.Interfaces;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Planes;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class PlanStatsService(GymManagerDbContext context) : IPlanStatsService
    {

        private readonly GymManagerDbContext _context = context;

        public async Task<StatsPlanRequest> GetStatsAsync()
        {
            ///Contamos la cantidad de Planes activos
            var cantidadPlanesActivos = await _context.Planes.Where(p => p.EliminadoEn == null).CountAsync();

            var socios = _context.Socios.Where(s => s.EliminadoEn == null);

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
