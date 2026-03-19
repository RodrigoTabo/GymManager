using GymManager.Api.Application.Interfaces;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Socios;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class SocioStatsService(GymManagerDbContext context,
        ICurrentUserService currentUserService) : ISocioStatsService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<SociosStatsResponse> GetStatsAsync()
        {
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            DateTime inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime inicioMesSiguiente = inicioMes.AddMonths(1);

            const int meses = 12;
            var inicioMesInicial = inicioMes.AddMonths(-(meses - 1));

            var labelsMeses = new List<string>(meses);
            var altasPorMes = new List<int>(meses);
            var bajasPorMes = new List<int>(meses);

            var ActivosCount = await _context.Socios
                .AsNoTracking()
                .Where(s => s.EliminadoEn == null && s.SucursalId == sucursalId)
                .CountAsync();

            var InactivosCount = await _context.Socios
                .AsNoTracking()
                .Where(s => s.EliminadoEn != null && s.SucursalId == sucursalId)
                .CountAsync();

            var AltasMesCount = await _context.Socios
                .AsNoTracking()
                .Where(s => s.FechaAlta >= inicioMes && s.FechaAlta < inicioMesSiguiente && s.EliminadoEn == null && s.SucursalId == sucursalId)
                .CountAsync();

            var BajasMesCount = await _context.Socios
                .AsNoTracking()
                .Where(s => s.EliminadoEn >= inicioMes && s.EliminadoEn < inicioMesSiguiente && s.SucursalId == sucursalId)
                .CountAsync();

            for (int i = 0; i < meses; i++)
            {
                var m = inicioMesInicial.AddMonths(i);
                var mSiguiente = m.AddMonths(1);

                labelsMeses.Add(m.ToString("MMM"));

                var altas = await _context.Socios
                    .AsNoTracking()
                    .Where(s => s.FechaAlta >= m && s.FechaAlta < mSiguiente && s.SucursalId == sucursalId)
                    .CountAsync();

                var bajas = await _context.Socios
                    .AsNoTracking()
                    .Where(s => s.EliminadoEn != null && s.EliminadoEn >= m && s.EliminadoEn < mSiguiente && s.SucursalId == sucursalId)
                    .CountAsync();

                altasPorMes.Add(altas);
                bajasPorMes.Add(bajas);
            }

            var stats = new SociosStatsResponse
            {
                ActivosCount = ActivosCount,
                InactivosCount = InactivosCount,
                AltasMesCount = AltasMesCount,
                BajasMesCount = BajasMesCount,
                Meses = labelsMeses,
                AltasPorMes = altasPorMes,
                BajasPorMes = bajasPorMes
            };

            return stats;

        }

    }
}
