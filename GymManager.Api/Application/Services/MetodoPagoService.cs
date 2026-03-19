using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.MetodoPago;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class MetodoPagoService(GymManagerDbContext context,
        ISucursalAccessValidator sucursalAccessValidator) : IMetodoPagoService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ISucursalAccessValidator _sucursalAccessValidator = sucursalAccessValidator;


        public async Task<int> CrearAsync(Guid sucursalid, CreateMetodoPagoRequest request)
        {

            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            ///Normalizamos y validamos
            var nombre = (request.Nombre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BadRequestException("Debes ingresar un Nombre");

            ///Buscamos si existe
            var existeMetodo = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Nombre == nombre && m.SucursalId == sucursalId);

            /// Es nullo?
            if (existeMetodo is not null)
            {
                /// Esta deshabilitado?
                if (existeMetodo.EliminadoEn != null)
                    throw new ConflictException("El metodo de pago existe, pero está deshabilitado.");

                throw new ConflictException("El metodo de pago ya existe.");
            }

            var crearMetodo = new MetodoPago
            {
                Nombre = nombre,
                EliminadoEn = null,
                SucursalId = sucursalId
            };

            _context.MetodosPago.Add(crearMetodo);

            await _context.SaveChangesAsync();

            return crearMetodo.Id;

        }

        public async Task<List<MetodoPagoResponse>> ListarAsync(Guid sucursalid)
        {

            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Consultamos => Trae el que no esta deshabilitado.
            var query = _context.MetodosPago
                .AsNoTracking()
                .Where(m => m.EliminadoEn == null && m.SucursalId == sucursalId);

            //Selecciono lo que quiero mostrar.
            var listar = await query
                .Select(m => new MetodoPagoResponse
                (
                    m.Id,
                    m.Nombre
                )).ToListAsync();

            return listar;
        }

        public async Task UpdateAsync(Guid sucursalid, UpdateMetodoPagoRequest request, int id)
        {

            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            var metodo = await _context.MetodosPago.FindAsync(id);

            if (metodo is null)
                throw new NotFoundException("El Metodo de pago que deseas editar no existe");

            if (metodo.EliminadoEn != null)
                throw new ConflictException("El metodo que deseas editar, esta deshabilitado");

            if (metodo.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada no coincide con la sucursal activa.");

            var nombre = (request.Nombre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BadRequestException("El nombre es necesario");

            var existeYa = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Nombre == nombre && m.Id != id && m.SucursalId == sucursalId);

            if (existeYa is not null)
                throw new ConflictException("Ya existe un metodo con este nombre.");

            metodo.Nombre = nombre;

            await _context.SaveChangesAsync();

        }


        public async Task SoftDeleteAsync(Guid sucursalid, int id)
        {

            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            var metodo = await _context.MetodosPago.FindAsync(id);

            if (metodo is null)
                throw new NotFoundException("El Metodo de pago que deseas elimiar no existe");

            if (metodo.EliminadoEn != null)
                throw new ConflictException("El Metodo ya esta eliminado");

            if (metodo.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada no coincide con la sucursal activa.");

            metodo.EliminadoEn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

        }

    }
}
