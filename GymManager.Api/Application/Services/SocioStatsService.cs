using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Socios;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace GymManager.Api.Application.Services
{
    public class SocioStatsService(GymManagerDbContext context,
        ISucursalAccessValidator sucursalAccessValidator) : ISocioStatsService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ISucursalAccessValidator _sucursalAccessValidator = sucursalAccessValidator;

        public async Task<SociosStatsResponse> GetStatsAsync(Guid sucursalid)
        {
            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

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

            //De lo que iba a ser uno de los graficos.
            //var CobroMestotal = await _context.Pagos
            //    .AsNoTracking()
            //    .Where(p => p.FechaPago >= inicioMes && p.FechaPago < inicioMesSiguiente && p.EliminadoEn == null)
            //    .SumAsync(p => p.Importe);

            //var ultimosPagos = await _context.Pagos
            // .AsNoTracking()
            // .Where(p =>
            //     p.FechaPago >= inicioMes &&
            //     p.FechaPago < inicioMesSiguiente &&
            //     p.EliminadoEn == null)
            // .OrderByDescending(p => p.FechaPago)
            // .Take(5)
            // .Select(p => new PagoResponse(
            //     p.Id,
            //     p.SocioId,
            //     p.Socio.Nombre,
            //     p.FechaPago,
            //     (decimal)p.Importe,
            //     p.MetodoPagoId,
            //     p.MetodoPago.Nombre,
            //     p.CubreDesde,
            //     p.CubreHasta
            // ))
            // .ToListAsync();


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
