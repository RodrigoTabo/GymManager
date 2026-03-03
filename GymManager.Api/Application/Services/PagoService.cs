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

        public Task<List<PagoResponse>> ListarAsync()
        {
            throw new NotImplementedException();
        }
    }

}
