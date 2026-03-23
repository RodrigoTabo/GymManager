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

            //Traemos la sucursal para comparar.
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Validamos si existe pero esta eliminado.
            var existe = await _context.Planes
                .FirstOrDefaultAsync(p => p.Nombre == request.Nombre && p.SucursalId == sucursalId);

            //Si existe
            await ValidarPlanExistenteAsync(request.Nombre, sucursalId);

            //Creamos el plan
            var nuevoPlan = new Plan
            {
                Nombre = request.Nombre,
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
            //Traemos la sucursalId para comparar.
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Validamos que exista el Id que queremos modificar.
            var plan = await ObtenerPlanAsync (id, sucursalId);

            //Validamos que el plan no exista
            await ValidarPlanExistenteAsync(request.Nombre, sucursalId, id);

            plan.Nombre = request.Nombre;
            plan.DuracionDias = request.DuracionDias;
            plan.Precio = request.Precio;
            await _context.SaveChangesAsync();

        }

        public async Task<PlanResponse> GetByIdAsync(int id)
        {
            //Traemos la sucursalId para comparar.
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

            //Buscamos dentro de la sucursal, un Id igual.
            var plan = await ObtenerPlanAsync(id, sucursalId);

            plan.EliminadoEn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

        }

        //METODOS PRIVADOS

        private async Task<Plan> ObtenerPlanAsync(int id, Guid sucursalId)
        {
            //Validamos que exista el Id que queremos modificar.
            var plan = await _context.Planes
                .FirstOrDefaultAsync(p => p.Id == id && p.SucursalId == sucursalId);

            //Si el plan no existe..
            if (plan is null)
                throw new NotFoundException("El Plan que desea modificar no existe.");

            //Si el plan existe, esta eliminado?
            if (plan.EliminadoEn != null)
                throw new ConflictException("El plan ya está deshabilitado.");

            return plan;
        }

        private async Task ValidarPlanExistenteAsync(string nombre, Guid sucursalId, int? id = null)
        {
            //Armamos uan query.
            var query = _context.Planes
                .AsQueryable()
                .Where(p => p.Nombre == nombre && p.SucursalId == sucursalId);

            //Si Id es diferente a null, lo agregamos a la query
            if (id != null)
                query.Where(p => p.Id != id.Value);

            var existe = await query.FirstOrDefaultAsync();

            if (existe is not null)
            {
                if (existe.EliminadoEn != null)
                    throw new ConflictException("El plan ya existe, pero está deshabilitado.");

                throw new ConflictException("El plan ya existe.");
            }

        }


    }
}
