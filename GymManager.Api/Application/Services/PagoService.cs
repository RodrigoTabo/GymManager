using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Pagos;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class PagoService(GymManagerDbContext context,
        ICurrentSucursalService currentSucursalService,
        ICurrentUserService currentUserService) : IPagoService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentSucursalService _currentSucursalService = currentSucursalService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<int> CrearAsync(Guid sucursalid, CreatePagoRequest request)
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

            //Buscamos un Socio que coincida con el ingresado
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.Id == request.SocioId && s.SucursalId == sucursalId);

            //Si no coincide => Not Found (404).
            if (socio is null)
                throw new NotFoundException("El Socio no existe.");
            //Coincide pero esta deshabilitado =>  Conflic (409)
            if (socio.EliminadoEn != null)
                throw new ConflictException("El socio está deshabilitado.");

            //Buscamos un MetodoPago que coincida con el ingresado
            var metodoExiste = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Id == request.MetodoPagoId && m.SucursalId == sucursalId);

            //Si no existe => Not Found (404)
            if (metodoExiste is null)
                throw new NotFoundException("El Metodo de Pago no existe.");
            //Si existe, esta deshabiltado => Conflicto(409)
            if (metodoExiste.EliminadoEn != null)
                throw new ConflictException("El Metodo de Pago esta deshabilitado.");

            //Guardamos el importe dependiendo el plan del usuario.
            var plan = await _context.Planes
                .FirstOrDefaultAsync(p => p.Id == socio.PlanId && p.SucursalId == sucursalId);
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
                EliminadoEn = null,
                SucursalId = sucursalId
            };

            _context.Pagos.Add(crearPago);

            await _context.SaveChangesAsync();

            return crearPago.Id;
        }      

        public async Task<List<PagoResponse>> ListarAsync(Guid sucursalid)
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

            //Optimizamos consulta y la filtramos.
            var consulta = _context.Pagos
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId);

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


        public async Task SoftDeleteAsync(Guid sucursalid, int id)
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

            //Buscamos el Pago
            var pagosExiste = await _context.Pagos.FindAsync(id);
            //Existe?
            if (pagosExiste == null)
                throw new NotFoundException("El Pago no existe.");
            //Existe, ya esta eliminado?
            if (pagosExiste.EliminadoEn != null)
                throw new ConflictException("El Pago ya esta eliminado.");

            if (pagosExiste.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Lo eliminamos
            pagosExiste.EliminadoEn = DateTime.UtcNow;
            //Guardamos.
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid sucursalid, UpdatePagoRequest request, int id)
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

            var pago = await _context.Pagos.FindAsync(id);

            if (pago is null)
                throw new NotFoundException("El Pago que queres editar no existe.");

            if (pago.EliminadoEn != null)
                throw new ConflictException("El Pago que queres editar esta eliminado.");

            if (pago.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Buscamos un MetodoPago que coincida con el ingresado
            var metodoExiste = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Id == request.MetodoPagoId && m.SucursalId == sucursalId);

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
