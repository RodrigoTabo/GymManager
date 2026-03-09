using GymManager.Api.Application.Interfaces;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class PagoStatsService (GymManagerDbContext context) : IPagoStatsService
    {

        private readonly GymManagerDbContext _context = context;

        public async Task<PagosStatsResponse> GetStatsAsync()
        {
            DateTime inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var inicioSerie = inicioMes.AddMonths(-5); // 6 meses: mes actual + 5 atrás
            DateTime inicioMesSiguiente = inicioMes.AddMonths(1);
            DateTime hoy = DateTime.UtcNow.Date;
            DateTime mañana = hoy.AddDays(1);

            // Base query reutilizable (soft delete)
            var pagosBase = _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null);

            //CantidadPagosMensuales
            var pagosMensualesCount = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago < inicioMesSiguiente && p.EliminadoEn == null).CountAsync();

            //TotalPagosMensuales
            var totalPagoMensual = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago < inicioMesSiguiente && p.EliminadoEn == null)
                .SumAsync(p => p.Importe);

            //CantidadPagosDiarios
            var pagosDiariosCount = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana && p.EliminadoEn == null).CountAsync();
            //TotalPagosDiarios
            var totalPagoDiario = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana && p.EliminadoEn == null)
                .SumAsync(p => p.Importe);

            //Buscamos efectivo y transferencia

            var metodos = await _context.MetodosPago.AsNoTracking()
                .Where(m => m.EliminadoEn == null && (m.Nombre == "Efectivo" || m.Nombre == "Transferencia"))
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
                .Where(p => p.EliminadoEn == null)
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana)
                .Where(p => p.MetodoPagoId == efectivoId.Value)
                .CountAsync();

            var totalDiarioPagoEfectivo = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null)
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana)
                .Where(p => p.MetodoPagoId == efectivoId.Value)
                .SumAsync(p => p.Importe);

            //Pagos diarios en transferencia
            var pagosDiariosEnTransferencia = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null)
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana)
                .Where(p => p.MetodoPagoId == transferenciaId.Value)
                .CountAsync();

            var totalDiarioPagoTransferencia = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null)
                .Where(p => p.FechaPago >= hoy && p.FechaPago < mañana)
                .Where(p => p.MetodoPagoId == transferenciaId.Value)
                .SumAsync(p => p.Importe);

            var serieRaw = await pagosBase
                .Where(p => p.FechaPago >= inicioSerie && p.FechaPago < mañana)
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
    }
}
