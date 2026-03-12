using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class PagoStatsService(GymManagerDbContext context,
        ICurrentSucursalService currentSucursalService,
        ICurrentUserService currentUserService) : IPagoStatsService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentSucursalService _currentSucursalService = currentSucursalService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<PagosStatsResponse> GetStatsAsync(Guid sucursalid)
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

            DateTime inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var inicioSerie = inicioMes.AddMonths(-5); // 6 meses: mes actual + 5 atrás
            DateTime inicioMesSiguiente = inicioMes.AddMonths(1);
            DateTime hoy = DateTime.UtcNow.Date;
            DateTime mañana = hoy.AddDays(1);

            // Base query reutilizable (soft delete)
            var pagosBase = _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId);

            //CantidadPagosMensuales
            var pagosMensualesCount = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago < inicioMesSiguiente && p.EliminadoEn == null && p.SucursalId == sucursalId).CountAsync();

            //TotalPagosMensuales
            var totalPagoMensual = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago < inicioMesSiguiente && p.EliminadoEn == null && p.SucursalId == sucursalId)
                .SumAsync(p => p.Importe);

            //CantidadPagosDiarios
            var pagosDiariosCount = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana && p.EliminadoEn == null && p.SucursalId == sucursalId).CountAsync();
            //TotalPagosDiarios
            var totalPagoDiario = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana && p.EliminadoEn == null && p.SucursalId == sucursalId)
                .SumAsync(p => p.Importe);

            //Buscamos efectivo y transferencia

            var metodos = await _context.MetodosPago.AsNoTracking()
                .Where(m => m.EliminadoEn == null && (m.Nombre == "Efectivo" || m.Nombre == "Transferencia") && m.SucursalId == sucursalId)
                .Select(m => new { m.Id, m.Nombre })
                .ToListAsync();

            var efectivoId = metodos.FirstOrDefault(x => x.Nombre == "Efectivo")?.Id;

            var transferenciaId = metodos.FirstOrDefault(x => x.Nombre == "Transferencia")?.Id;

            if (efectivoId is null)
                throw new NotFoundException("No existe el método de pago 'Efectivo'.");

            if (transferenciaId is null)
                throw new NotFoundException("No existe el método de pago 'Transferencia'.");

            //PagosEnEfectivo
            var pagosDiariosEnEfectivo = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId)
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana && p.SucursalId == sucursalId)
                .Where(p => p.MetodoPagoId == efectivoId.Value && p.SucursalId == sucursalId)
                .CountAsync();

            var totalDiarioPagoEfectivo = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId)
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana && p.SucursalId == sucursalId)
                .Where(p => p.MetodoPagoId == efectivoId.Value && p.SucursalId == sucursalId)
                .SumAsync(p => p.Importe);

            //Pagos diarios en transferencia
            var pagosDiariosEnTransferencia = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId)
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana && p.SucursalId == sucursalId)
                .Where(p => p.MetodoPagoId == transferenciaId.Value && p.SucursalId == sucursalId)
                .CountAsync();

            var totalDiarioPagoTransferencia = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId)
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana && p.SucursalId == sucursalId)
                .Where(p => p.MetodoPagoId == transferenciaId.Value && p.SucursalId == sucursalId)
                .SumAsync(p => p.Importe);

            var serieRaw = await pagosBase
                .Where(p => p.FechaPago >= inicioSerie && p.FechaPago < mañana && p.SucursalId == sucursalId)
                .GroupBy(p => new { p.FechaPago.Year, p.FechaPago.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.Importe),
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();


            var dic = serieRaw.ToDictionary(
                x => $"{x.Year:D4}-{x.Month:D2}",
                x => new PagoMesSerie($"{x.Year:D4}-{x.Month:D2}", x.Total, x.Cantidad)
            );

            var totalPorMes = new List<PagoMesSerie>(capacity: 6);
            var cursor = new DateTime(inicioSerie.Year, inicioSerie.Month, 1);


            for (int i = 0; i < 6; i++)
            {
                var key = $"{cursor.Year:D4}-{cursor.Month:D2}";
                totalPorMes.Add(dic.TryGetValue(key, out var v)
                    ? v
                    : new PagoMesSerie(key, 0m, 0));

                cursor = cursor.AddMonths(1);
            }

            var stats = new PagosStatsResponse
            {
                PagosMensualesCount = pagosMensualesCount,
                PagosDiariosCount = pagosDiariosCount,
                TotalPagoMensual = totalPagoMensual,
                TotalPagoDiario = totalPagoDiario,
                PagosDiariosEnEfectivo = pagosDiariosEnEfectivo,
                TotalDiariosPagoEfectivo = totalDiarioPagoEfectivo,
                PagosDiariosEnTransferencia = pagosDiariosEnTransferencia,
                TotalDiariosPagoTransferencia = totalDiarioPagoTransferencia,
                TotalPorMes = totalPorMes
            };

            return stats;
        }

        public async Task<VencimientoStatsResponse> GetVencidosStatsAsync(Guid sucursalid)
        {
            //Validamos las sucursalId y el userId.
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

            DateTime hoy = DateTime.UtcNow.Date;
            DateTime mañana = hoy.AddDays(1);
            DateTime semana = hoy.AddDays(+7);

            var ultimosVencimientos = _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId)
                .Where(p => p.Socio.EliminadoEn == null)
                .GroupBy(p => p.SocioId)
                .Select(g => new
                {
                    SocioId = g.Key,
                    CubreHasta = g.Max(p => p.CubreHasta)
                });

            var VencenHoy = await _context.Pagos
                .Where(p => p.EliminadoEn == null)
                .GroupBy(p => p.SocioId)
                .Select(g => g.Max(p => p.CubreHasta))
                .CountAsync(cubreHasta => cubreHasta >= hoy && cubreHasta < mañana);

            var VencenEstaSemana = await _context.Pagos
                .Where(p => p.EliminadoEn == null)
                .GroupBy(p => p.SocioId)
                .Select(g => g.Max(p => p.CubreHasta))
                .CountAsync(cubreHasta => cubreHasta >= hoy && cubreHasta < semana);

            //var TotalCobrarHoy = await _context.Pagos
            //    .AsNoTracking()
            //    .Where(p => p.EliminadoEn == null)
            //    .Where(p => p.CubreHasta >= hoy && p.CubreHasta < mañana && p.SucursalId == sucursalId)
            //    .Where(s => s.Socio.EliminadoEn == null && s.SucursalId == sucursalId)
            //    .SumAsync(p => p.Importe);

            var TotalCobrarHoy = await (
                from uv in ultimosVencimientos
                join s in _context.Socios.AsNoTracking()
                    on uv.SocioId equals s.Id
                where uv.CubreHasta >= hoy
                   && uv.CubreHasta < mañana
                   && s.SucursalId == sucursalId
                   && s.EliminadoEn == null
                select (decimal?)s.Plan.Precio
            ).SumAsync() ?? 0m;

            var TotalCobrarSemana = await (
                from uv in ultimosVencimientos
                join s in _context.Socios.AsNoTracking()
                    on uv.SocioId equals s.Id
                where uv.CubreHasta >= hoy
                   && uv.CubreHasta < semana
                   && s.SucursalId == sucursalId
                   && s.EliminadoEn == null
                select (decimal?)s.Plan.Precio
            ).SumAsync() ?? 0m;

            var vencidos = await _context.Socios
                .AsNoTracking()
                .Where(p => p.FechaBaja != null && p.SucursalId == sucursalId)
                .CountAsync();

            var stats = new VencimientoStatsResponse
            {
                VencenHoy = VencenHoy,
                VencenEstaSemana = VencenEstaSemana,
                TotalCobrarHoy = TotalCobrarHoy,
                TotalCobrarSemana = TotalCobrarSemana,
                Vencidos = vencidos
            };

            return stats;

        }
    }
}
