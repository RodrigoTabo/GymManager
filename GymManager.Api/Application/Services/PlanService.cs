using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Planes;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class PlanService(GymManagerDbContext context,
        ICurrentUserService currentUserService) : IPlanService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<List<PlanResponse>> ListarAsync()
        {

            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Optimizamos la consulta que vamos a realizar.
            var query = _context.Planes
                .AsNoTracking()
                .Where(p => p.EliminadoEn == null && p.SucursalId == sucursalId);

            //Realizamos la consulta con la query optimizada.
            var listar = await query
                .OrderByDescending(x => x.Precio)
                .Select(p => new PlanResponse
                (
                    p.Id,
                    p.Nombre,
                    p.DuracionDias,
                    p.Precio
                )).ToListAsync();
            //Retornamos la lista de Plan

            return listar;
        }

        public async Task<int> CrearAsync(CreatePlanRequest request)
        {
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Validamos que haya cargado el nombre
            var nombre = (request.Nombre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BadRequestException("El nombre es obligatorio.");

            //Validamos que haya colocado duración de dias
            if (request.DuracionDias <= 0)
                throw new BadRequestException("Tenes que agregar los días de duración del plan.");
            //Validamos que haya colocado precio al plan
            if (request.Precio <= 0)
                throw new BadRequestException("Tenes que agregar un precio al plan.");

            //Validamos si existe pero esta eliminado.
            var existe = await _context.Planes
                .FirstOrDefaultAsync(p => p.Nombre == nombre && p.SucursalId == sucursalId);

            if (existe is not null)
            {
                if (existe.EliminadoEn != null)
                    throw new ConflictException("El plan ya existe, pero está deshabilitado.");

                throw new ConflictException("El plan ya existe.");
            }

            //Creamos el plan
            var nuevoPlan = new Plan
            {
                Nombre = nombre,
                DuracionDias = request.DuracionDias,
                Precio = request.Precio,
                SucursalId = sucursalId,
            };

            //Agregamos el nuevo plan
            await _context.Planes.AddAsync(nuevoPlan);
            //Guardamos el nuevo plan
            await _context.SaveChangesAsync();
            //Retornamos el Id del plan
            return nuevoPlan.Id;

        }

        public async Task UpdateAsync(int id, UpdatePlanRequest request)
        {
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Validamos que exista el Id que queremos modificar.
            var plan = await _context.Planes.FindAsync(id);
            if (plan is null)
                throw new NotFoundException("El Plan que desea modificar no existe.");

            if (plan.EliminadoEn != null)
                throw new ConflictException("El plan ya está deshabilitado.");

            if (plan.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Validamos que haya completado los campos
            var nombre = (request.Nombre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BadRequestException("El nombre es obligatorio.");
            if (request.DuracionDias <= 0)
                throw new BadRequestException("Tenes que agregar los días de duración del plan");
            if (request.Precio <= 0)
                throw new BadRequestException("Tenes que agregar un precio al plan");

            //Validamos que el plan no exista
            var existe = await _context.Planes
                .FirstOrDefaultAsync(p => p.Nombre == nombre && p.Id != id && p.SucursalId == sucursalId);

            if (existe is not null)
            {
                if (existe.EliminadoEn != null)
                    throw new ConflictException("El plan ya existe, pero está deshabilitado.");

                throw new ConflictException("El plan ya existe.");
            }

            plan.Nombre = nombre;
            plan.DuracionDias = request.DuracionDias;
            plan.Precio = request.Precio;
            await _context.SaveChangesAsync();

        }

        public async Task<PlanResponse> GetByIdAsync(int id)
        {
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            ///Buscamos el plan.
            var plan = await _context.Planes
                .AsNoTracking()
                .Where(p => p.Id == id && p.EliminadoEn == null && p.SucursalId == sucursalId)
                .Select(p => new PlanResponse
                (
                    p.Id,
                    p.Nombre,
                    p.DuracionDias,
                    p.Precio
                )).FirstOrDefaultAsync();
            ///Si no existe.
            if (plan is null)
                throw new NotFoundException("El plan no existe.");

            return plan;

        }

        public async Task SoftDeleteAsync(int id)
        {
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            var plan = await _context.Planes.FindAsync(id);

            ///Existe el plan?
            if (plan is null)
                throw new NotFoundException("El plan no existe.");
            ///Esta deshabilitado?
            if (plan.EliminadoEn != null)
                throw new ConflictException("El plan ya está deshabilitado.");

            if (plan.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            plan.EliminadoEn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

        }

    }
}
