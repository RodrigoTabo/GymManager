using Azure.Core;
using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class PagoService(GymManagerDbContext context,
        ICurrentUserService currentUserService) : IPagoService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<int> CrearAsync(CreatePagoRequest request)
        {
            //Traer sucursal siempre traer sucursal
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Obtengo el socio valido
            var socio = await ObtenerSocioValidoAsync(request.SocioId, sucursalId);

            //Obtengo el metodo valido
            await ObtenerMetodoPagoValidoAsync(request.MetodoPagoId, sucursalId);

            //Obtenemos el plan valido
            var plan = await ObtenerPlanValidoAsync(socio, sucursalId);

            //Obtenemos calcular cobertura.
            var (cubreDesde, cubreHasta) = CalcularCobertura(plan.DuracionDias);

            // Si eligio un plan, colocamos el precio del plan y lo que pago.
            var importe = plan.Precio;

            var pago = CrearEntidadPago(request, sucursalId, importe, cubreDesde, cubreHasta);

            _context.Pagos.Add(pago);

            await _context.SaveChangesAsync();

            return pago.Id;
        }

        public async Task<List<PagoResponse>> ListarAsync()
        {
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Optimizamos consulta y la filtramos.
            var consulta = _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId);

            //Pedimos la consulta y la guardamos como lista.
            var listar = await consulta
                .OrderByDescending(p => p.FechaPago)
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
                    ))
                .ToListAsync();


            //Retornamos la lista.
            return listar;

        }

        public async Task SoftDeleteAsync(int id)
        {

            //Traemos sucursales para comparar
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Obtenemos el pago valido
            var pago = await ObtenerPagoValidoAsync(id, sucursalId);

            //Lo eliminamos
            pago.EliminadoEn = DateTime.UtcNow;
            //Guardamos.
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdatePagoRequest request, int id)
        {

            //Traemos la sucursal para comparar.
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Obtenemos el pago valido
            var pago = await ObtenerPagoValidoAsync(id, sucursalId);

            //Obtengo el metodo valido
            var metodoPago = await ObtenerMetodoPagoValidoAsync(request.MetodoPagoId, sucursalId);

            //Obtengo ValidarFecha
            ValidarFecha(request.FechaPago);

            pago.MetodoPagoId = request.MetodoPagoId;
            pago.FechaPago = request.FechaPago;

            await _context.SaveChangesAsync();

        }

        public async Task<List<VencidoResponse>> GetVencidosAsync()
        {

            //Traemos la sucursal para comparar.
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            DateTime hoy = DateTime.UtcNow.Date;
            DateTime semana = hoy.AddDays(+7);


            var pagosFiltrados = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.Socio.EliminadoEn == null)
                .Where(p => p.EliminadoEn == null
                    && p.SucursalId == sucursalId)
                .Select(p => new
                {
                    p.Id,
                    p.SocioId,
                    p.FechaPago,
                    p.CubreHasta,
                    p.Socio.Plan.Precio,
                    Nombre = p.Socio.Nombre,
                    Apellido = p.Socio.Apellido,
                    Plan = p.Socio.Plan.Nombre,
                    Importe = p.Socio.Plan.Precio,
                    Telefono = p.Socio.Telefono
                })
                .ToListAsync();

            // Resuelvo en memoria porque E.F no "soportaba" la consulta
            var listar = pagosFiltrados
                .GroupBy(p => p.SocioId)
                .Select(g => g
                    .OrderByDescending(x => x.FechaPago)
                    .ThenByDescending(x => x.Id)
                    .First())
                .Where(p => p.CubreHasta < semana)
                .OrderBy(x => x.CubreHasta)
                .Select(x => new VencidoResponse
                {
                    NombreCompleto = x.Nombre + " " + x.Apellido,
                    SocioId = x.SocioId,
                    Plan = x.Plan,
                    VenceEn = x.CubreHasta,
                    Importe = x.Precio,
                    Telefono = x.Telefono
                })
                .ToList();

            return listar;

        }

        // METODOS PRIVADOS
        //Aprendiendo a desacoplar perfectamente
        private async Task<Socio> ObtenerSocioValidoAsync(int socioId, Guid sucursalId)
        {
            //Buscamos un Socio que coincida con el ingresado
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.Id == socioId && s.SucursalId == sucursalId);

            //Si no coincide => Not Found (404).
            if (socio is null)
                throw new NotFoundException("El Socio no existe.");

            //Coincide pero esta deshabilitado =>  Conflic (409)
            if (socio.EliminadoEn != null)
                throw new ConflictException("El socio está deshabilitado.");

            return socio;
        }

        private async Task<MetodoPago> ObtenerMetodoPagoValidoAsync(int metodoPagoId, Guid sucursalId)
        {

            //Buscamos un MetodoPago que coincida con el ingresado
            var metodo = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Id == metodoPagoId && m.SucursalId == sucursalId);

            //Si no existe => Not Found (404)
            if (metodo is null)
                throw new NotFoundException("El Metodo de Pago no existe.");

            //Si existe, esta deshabiltado => Conflicto(409)
            if (metodo.EliminadoEn != null)
                throw new ConflictException("El Metodo de Pago esta deshabilitado.");

            return metodo;
        }

        private async Task<Plan> ObtenerPlanValidoAsync(Socio socio, Guid sucursalId)
        {

            //Guardamos el importe dependiendo el plan del usuario.
            var plan = await _context.Planes
                .FirstOrDefaultAsync(p => p.Id == socio.PlanId && p.SucursalId == sucursalId);

            //Si el importe es menor o 0, entonces el socio no tiene ningun Plan asignado.
            if (plan is null)
                throw new BadRequestException("El Socio no tiene ningun Plan asignado.");

            //Si eliminado esta cargado.
            if (plan.EliminadoEn != null)
                throw new ConflictException("El plan está deshabilitado.");

            //Si quieren romper aproposito
            if (plan.DuracionDias <= 0)
                throw new ConflictException("Hubo un problema con la duración de días del plan");

            return plan;

        }

        private async Task<Pago> ObtenerPagoValidoAsync(int id, Guid sucursalId)
        {

            //Buscamos el Pago y filtramos por sucursalId
            var pago = await _context.Pagos
                .FirstOrDefaultAsync(p => p.Id == id && p.SucursalId == sucursalId);
            //Si es null...
            if (pago is null)
                throw new NotFoundException("El Pago que queres editar no existe.");
            //Si existe pero esta eliminado...
            if (pago.EliminadoEn != null)
                throw new ConflictException("El Pago que queres editar esta eliminado.");

            return pago;

        }

        private void ValidarFecha(DateTime fecha)
        {
            //Validamos si ingreso fecha.
            if (fecha == default)
                throw new BadRequestException("Fecha inválida.");
        }

        private (DateTime desde, DateTime hasta) CalcularCobertura(int duracionDias)
        {
            //Calculamos la fecha ingresada para su cobertura.
            var desde = DateTime.UtcNow;
            var hasta = desde.AddDays(duracionDias);

            return (desde, hasta);
        }

        private Pago CrearEntidadPago(CreatePagoRequest request, Guid sucursalId, decimal importe, DateTime cubreDesde, DateTime cubreHasta)
        {
            return new Pago
            {
                Importe = importe,
                FechaPago = DateTime.UtcNow,
                CubreDesde = cubreDesde,
                CubreHasta = cubreHasta,
                MetodoPagoId = request.MetodoPagoId,
                SocioId = request.SocioId,
                EliminadoEn = null,
                SucursalId = sucursalId
            };
        }

    }

}
