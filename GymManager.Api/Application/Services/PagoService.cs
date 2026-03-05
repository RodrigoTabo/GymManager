using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class PagoService(GymManagerDbContext context) : IPagoService
    {
        private readonly GymManagerDbContext _context = context;

        public async Task<int> CrearAsync(CreatePagoRequest request)
        {

            //Buscamos un Socio que coincida con el ingresado
            var socio = await _context.Socios.FirstOrDefaultAsync(s => s.Id == request.SocioId);

            //Si no coincide => Not Found (404).
            if (socio is null)
                throw new NotFoundException("El Socio no existe.");
            //Coincide pero esta deshabilitado =>  Conflic (409)
            if (socio.EliminadoEn != null)
                throw new ConflictException("El socio está deshabilitado.");

            //Buscamos un MetodoPago que coincida con el ingresado
            var metodoExiste = await _context.MetodosPago.FirstOrDefaultAsync(m => m.Id == request.MetodoPagoId);
            //Si no existe => Not Found (404)
            if (metodoExiste is null)
                throw new NotFoundException("El Metodo de Pago no existe.");
            //Si existe, esta deshabiltado => Conflicto(409)
            if (metodoExiste.EliminadoEn != null)
                throw new ConflictException("El Metodo de Pago esta deshabilitado.");

            //Guardamos el importe dependiendo el plan del usuario.
            var plan = await _context.Planes.FirstOrDefaultAsync(p => p.Id == socio.PlanId);
            //Si el importe es menor o 0, entonces el socio no tiene ningun Plan asignado.
            if (plan is null)
                throw new BadRequestException("El Socio no tiene ningun Plan asignado.");

            if (plan.EliminadoEn != null)
                throw new ConflictException("El plan está deshabilitado.");

            if (plan.DuracionDias <= 0)
                throw new BadRequestException("Hubo un problema con la duración de días");

            //Calculamos desde cuando cubre el dia, hasta cuando.
            var CubreDesde = DateTime.UtcNow;
            var CubreHasta = CubreDesde.AddDays(plan.DuracionDias - 1);
            // Si eligio un plan, colocamos el precio del plan y lo que pago.
            var importe = plan.Precio;


            var crearPago = new Pago
            {
                Importe = importe,
                FechaPago = DateTime.UtcNow,
                CubreDesde = CubreDesde,
                CubreHasta = CubreHasta,
                MetodoPagoId = request.MetodoPagoId,
                SocioId = request.SocioId,
                EliminadoEn = null
            };

            _context.Pagos.Add(crearPago);

            await _context.SaveChangesAsync();

            return crearPago.Id;
        }

        public async Task<PagosStatsResponse> GetPagosStatsAsync()
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

        public async Task<List<PagoResponse>> ListarAsync()
        {
            //Optimizamos consulta y la filtramos.
            var consulta = _context.Pagos.AsNoTracking().Where(p => p.EliminadoEn == null);

            //Pedimos la consulta y la guardamos como lista.
            var listar = await consulta
                .Select(p => new PagoResponse
                (
                    p.Id,
                    p.SocioId,
                    p.Socio.Nombre + " " + p.Socio.Apellido,
                    p.FechaPago,
                    p.Importe,
                    p.MetodoPagoId,
                    p.MetodoPago.Nombre,
                    p.CubreDesde,
                    p.CubreHasta
                    )).ToListAsync();

            //Retornamos la lista.
            return listar;

        }


        public async Task SoftDeleteAsync(int id)
        {
            //Buscamos el Pago
            var pagosExiste = await _context.Pagos.FindAsync(id);
            //Existe?
            if (pagosExiste == null)
                throw new NotFoundException("El Pago no existe.");
            //Existe, ya esta eliminado?
            if (pagosExiste.EliminadoEn != null)
                throw new ConflictException("El Pago ya esta eliminado.");
            //Lo eliminamos
            pagosExiste.EliminadoEn = DateTime.UtcNow;
            //Guardamos.
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdatePagoRequest request, int id)
        {

            var pago = await _context.Pagos.FindAsync(id);

            if (pago is null)
                throw new NotFoundException("El Pago que queres editar no existe.");

            if (pago.EliminadoEn != null)
                throw new ConflictException("El Pago que queres editar esta eliminado.");

            //Buscamos un MetodoPago que coincida con el ingresado
            var metodoExiste = await _context.MetodosPago.FirstOrDefaultAsync(m => m.Id == request.MetodoPagoId);
            //Si no existe => Not Found (404)
            if (metodoExiste is null)
                throw new NotFoundException("El Metodo de Pago no existe.");
            //Si existe, esta deshabiltado => Conflicto(409)
            if (metodoExiste.EliminadoEn != null)
                throw new ConflictException("El Metodo de Pago esta deshabilitado.");

            // Fecha válida (opcional pero recomendable)
            if (request.FechaPago == default)
                throw new BadRequestException("La fecha de pago es inválida.");

            pago.MetodoPagoId = request.MetodoPagoId;
            pago.FechaPago = request.FechaPago;

            await _context.SaveChangesAsync();

        }
    }




}
