using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.General;
using GymManager.Shared.Contracts.Pagos;
using GymManager.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class GeneralService(GymManagerDbContext context,
        ICurrentUserService currentUserService) : IGeneralService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<GeneralResponse> GetStatsGeneralAsync()
        {

            var sucursalId = _currentUserService.SucursalIdOrThrow;

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
                .Where(a => a.FechaRegistro >= hoy && a.SucursalId == sucursalId)
                .CountAsync();

            //Buscamos cantidad de asistidos en todo el mes
            var AsistidosMesCount = await _context.Asistencias
                .AsNoTracking()
                .Where(a => a.FechaRegistro >= inicioMes && a.FechaRegistro <= inicioMesSiguiente && a.SucursalId == sucursalId)
                .CountAsync();

            //Buscamos la cantidad de pagos hoy
            var PagosDiariosCount = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= hoy &&
                p.EliminadoEn == null &&
                p.SucursalId == sucursalId)
                .CountAsync();

            //Calculamos la cantidad que se recaudo hoy
            var TotalPagoDiario = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= hoy
                && p.EliminadoEn == null && p.SucursalId == sucursalId)
                .SumAsync(p => p.Importe);

            //Buscamos la cantidad de este mes
            var PagosMensualesCount = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago <= inicioMesSiguiente &&
                 p.EliminadoEn == null && p.SucursalId == sucursalId)
                .CountAsync();

            //Calculamos la cantidad que se recaudo este mes
            var TotalPagoMensual = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.FechaPago >= inicioMes && p.FechaPago <= inicioMesSiguiente &&
                 p.EliminadoEn == null && p.SucursalId == sucursalId)
                .SumAsync(p => p.Importe);


            //Calculamos Intentos de Asistencias hoy
            var CantidadIntentosHoyCount = await _context.IntentosAccesos
                .AsNoTracking()
                .Where(i => i.FechaRegistro >= hoy && i.Resultado == ResultadoAcceso.Denegada && i.SucursalId == sucursalId)
                .CountAsync();

            //Calculamos Intentos de Asistencia de todo el mes
            var CantidadIntentosMesCount = await _context.IntentosAccesos
                .AsNoTracking()
                .Where(i => i.FechaRegistro >= inicioMes && i.FechaRegistro <= inicioMesSiguiente && i.Resultado == ResultadoAcceso.Denegada && i.SucursalId == sucursalId)
                .CountAsync();

            //Listamos los que vencen hoy
            var ListaHoy = await _context.Pagos
                .AsNoTracking()
                .Include(p => p.Socio)
                    .ThenInclude(s => s.Plan)
                .Where(p =>
                p.Socio.EliminadoEn == null &&
                p.EliminadoEn == null &&
                p.SucursalId == sucursalId)
                .ToListAsync();

            var TopVencenHoy = ListaHoy
                .GroupBy(p => p.SocioId)
                .Select(g => g
                    .OrderByDescending(x => x.FechaPago)
                    .ThenByDescending(x => x.Id)
                    .First())
                .Where(p =>
                    p.CubreHasta >= hoy &&
                    p.CubreHasta <= mañana)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Importe)
                .Take(3)
                .Select(p => new VencidoResponse
                {
                    NombreCompleto = p.Socio.Nombre + " " + p.Socio.Apellido,
                    Importe = p.Importe,
                    Plan = p.Socio.Plan.Nombre,
                    VenceEn = p.CubreHasta,
                    Telefono = p.Socio.Telefono,
                })
                .ToList();

            //Listamos los que vencen en todo el mes
            var ListaMes = await _context.Pagos
            .AsNoTracking()
            .Include(p => p.Socio)
                .ThenInclude(s => s.Plan)
            .Where(p =>
            p.Socio.EliminadoEn == null &&
            p.EliminadoEn == null &&
            p.SucursalId == sucursalId)
            .ToListAsync();

            var TotalVencenMes = ListaMes
            .GroupBy(p => p.SocioId)
            .Select(g => g
                .OrderByDescending(x => x.FechaPago)
                .ThenByDescending(x => x.Id)
                .First())
            .Where(p =>
                p.CubreDesde >= inicioMes &&
                p.CubreHasta <= inicioMesSiguiente)
            .OrderByDescending(p => p.FechaPago)
            .ThenByDescending(x => x.Importe)
            .Take(3)
            .Select(p => new VencidoResponse
            {
                NombreCompleto = p.Socio.Nombre + " " + p.Socio.Apellido,
                Importe = p.Importe,
                Plan = p.Socio.Plan.Nombre,
                VenceEn = p.CubreHasta,
                Telefono = p.Socio.Telefono
            })
            .ToList();

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
                TopVencenHoy = TopVencenHoy,
                TopVencenMes = TotalVencenMes,
            };

            return stats;

        }
    }
}
