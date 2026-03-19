using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class PagoStatsService(GymManagerDbContext context,
         ICurrentUserService currentUserService) : IPagoStatsService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<PagosStatsResponse> GetStatsAsync()
        {
            var sucursalId = _currentUserService.SucursalIdOrThrow;

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
                .Where(p => p.FechaPago >= inicioSerie && p.FechaPago < mañana)
                .GroupBy(p => new { p.FechaPago.Year, p.FechaPago.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.Importe),
                    Cantidad = g.Count(),
                    TotalEfectivo = g
                        .Where(x => x.MetodoPagoId == efectivoId.Value)
                        .Sum(x => x.Importe),
                    TotalTransferencia = g
                        .Where(x => x.MetodoPagoId == transferenciaId.Value)
                        .Sum(x => x.Importe)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();


            var dic = serieRaw.ToDictionary(
        x => $"{x.Year:D4}-{x.Month:D2}",
        x => new PagoMesSerie(
            $"{x.Year:D4}-{x.Month:D2}",
            x.Total,
            x.Cantidad,
            x.TotalEfectivo,
            x.TotalTransferencia
        )
    );

            var totalPorMes = new List<PagoMesSerie>(capacity: 6);
            var cursor = new DateTime(inicioSerie.Year, inicioSerie.Month, 1);

            for (int i = 0; i < 6; i++)
            {
                var key = $"{cursor.Year:D4}-{cursor.Month:D2}";
                totalPorMes.Add(dic.TryGetValue(key, out var v)
                    ? v
                    : new PagoMesSerie(key, 0m, 0, 0m, 0m));

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

        public async Task<VencimientoStatsResponse> GetVencidosStatsAsync()
        {
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var finSemana = hoy.AddDays(7);

            var sociosConPagoActual = await _context.Socios
                .AsNoTracking()
                .Where(s => s.SucursalId == sucursalId)
                .Where(s => s.EliminadoEn == null)
                .Where(s => s.FechaBaja == null)
                .Select(s => new
                {
                    SocioId = s.Id,
                    Nombre = s.Nombre,
                    PagoActual = _context.Pagos
                        .Where(p => p.SocioId == s.Id)
                        .Where(p => p.SucursalId == sucursalId)
                        .Where(p => p.EliminadoEn == null)
                        .OrderByDescending(p => p.FechaPago)
                        .ThenByDescending(p => p.Id)
                        .Select(p => new
                        {
                            p.CubreHasta,
                            Precio = s.Plan.Precio
                        })
                        .FirstOrDefault()
                })
                .Where(x => x.PagoActual != null)
                .ToListAsync();

            var normalizada = sociosConPagoActual
                .Select(x => new
                {
                    x.SocioId,
                    x.Nombre,
                    PrecioPlan = x.PagoActual!.Precio,
                    Vence = DateOnly.FromDateTime(x.PagoActual.CubreHasta)
                })
                .ToList();

            var vencenHoyItems = normalizada
                .Where(x => x.Vence == hoy)
                .ToList();

            var vencenEstaSemanaItems = normalizada
                .Where(x => x.Vence > hoy && x.Vence <= finSemana)
                .ToList();

            var vencidosItems = normalizada
                .Where(x => x.Vence < hoy)
                .ToList();

            return new VencimientoStatsResponse
            {
                VencenHoy = vencenHoyItems.Count,
                VencenEstaSemana = vencenEstaSemanaItems.Count,
                TotalCobrarHoy = vencenHoyItems.Sum(x => x.PrecioPlan),
                TotalCobrarSemana = vencenEstaSemanaItems.Sum(x => x.PrecioPlan),
                Vencidos = vencidosItems.Count
            };
        }
    }
}
