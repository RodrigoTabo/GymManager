using GymManager.Api.Application.Interfaces;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.General;
using GymManager.Shared.Contracts.Pagos;
using GymManager.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class GeneralService(GymManagerDbContext context) : IGeneralService
    {
        private readonly GymManagerDbContext _context = context;

        public async Task<GeneralResponse> GetStatsGeneralAsync(Guid sucursalId)
        {
            //Declaramos que hoy es hoy...
            DateTime hoy = DateTime.UtcNow.Date;
            DateTime mañana = hoy.AddDays(1);
            //Declaramos principio de mes
            DateTime inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            //Declaramos fin de mes
            DateTime inicioMesSiguiente = inicioMes.AddMonths(1);

            //Buscamos la cantidad de asistidos hoy
            var AsistidosHoyCount = await _context.Asistencias
                .AsNoTracking()
                .Where(a => a.FechaRegistro >= hoy)
                .CountAsync();

            //Buscamos cantidad de asistidos en todo el mes
            var AsistidosMesCount = await _context.Asistencias
                .AsNoTracking()
                .Where(a => a.FechaRegistro >= inicioMes && a.FechaRegistro <= inicioMesSiguiente)
                .CountAsync();

            //Buscamos la cantidad de pagos hoy
            var PagosDiariosCount = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= hoy &&
                p.EliminadoEn == null)
                .CountAsync();

            //Calculamos la cantidad que se recaudo hoy
            var TotalPagoDiario = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= hoy)
                .SumAsync(p => p.Importe);

            //Buscamos la cantidad de este mes
            var PagosMensualesCount = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago <= inicioMesSiguiente &&
                 p.EliminadoEn == null)
                .CountAsync();

            //Calculamos la cantidad que se recaudo este mes
            var TotalPagoMensual = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago <= inicioMesSiguiente &&
                 p.EliminadoEn == null)
                .SumAsync(p => p.Importe);


            //Calculamos Intentos de Asistencias hoy
            var CantidadIntentosHoyCount = await _context.IntentosAccesos
                .AsNoTracking()
                .Where(i => i.FechaRegistro >= hoy && i.Resultado == ResultadoAcceso.Denegada)
                .CountAsync();

            //Calculamos Intentos de Asistencia de todo el mes
            var CantidadIntentosMesCount = await _context.IntentosAccesos
                .AsNoTracking()
                .Where(i => i.FechaRegistro >= inicioMes && i.FechaRegistro <= inicioMesSiguiente && i.Resultado == ResultadoAcceso.Denegada)
                .CountAsync();

            //Listamos los que vencen hoy
            var TotalVencenHoy = await _context.Pagos
            .AsNoTracking()
            .Where(p =>
                p.CubreHasta >= hoy && p.CubreHasta <= mañana &&
                p.EliminadoEn == null)
            .OrderByDescending(p => p.FechaPago)
            .Take(5)
            .Select(p => new PagosVencidos
            {
                PagoId = p.Id,
                NombreSocio = p.Socio.Nombre + " " + p.Socio.Apellido,
                Precio = p.Importe,
                FechaVencimiento = p.CubreHasta
            }
            )
            .ToListAsync();

            //Listamos los que vencen en todo el mes
            var TotalVencenMes = await _context.Pagos
            .AsNoTracking()
            .Where(p => p.CubreDesde >= inicioMes && p.CubreHasta <= inicioMesSiguiente &&
                p.EliminadoEn == null)
            .OrderByDescending(p => p.FechaPago)
            .Take(5)
            .Select(p => new PagosVencidos
            {
                PagoId = p.Id,
                NombreSocio = p.Socio.Nombre + " " + p.Socio.Apellido,
                Precio = p.Importe,
                FechaVencimiento = p.CubreHasta
            }
            )
            .ToListAsync();

            var stats = new GeneralResponse
            {
                AsistidosHoyCount = AsistidosHoyCount,
                AsistidosMesCount = AsistidosMesCount,
                CantidadIntentosHoyCount = CantidadIntentosHoyCount,
                CantidadIntentosMesCount = CantidadIntentosMesCount,
                PagosDiariosCount = PagosDiariosCount,
                PagosMensualesCount = PagosMensualesCount,
                TotalPagoDiario = TotalPagoDiario,
                TotalPagoMensual = TotalPagoMensual,
                TopVencenHoy = TotalVencenHoy,
                TopVencenMes = TotalVencenMes,
            };

            return stats;

        }
    }
}
