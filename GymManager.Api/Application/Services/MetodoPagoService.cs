using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.MetodoPago;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class MetodoPagoService(GymManagerDbContext context) : IMetodoPagoService
    {
        private readonly GymManagerDbContext _context = context;


        public async Task<int> CrearAsync(CreateMetodoPagoRequest request)
        {

            ///Normalizamos y validamos
            var nombre = (request.Nombre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BadRequestException("Debes ingresar un Nombre");

            ///Buscamos si existe
            var existeMetodo = await _context.MetodosPago.FirstOrDefaultAsync(m => m.Nombre == nombre);

            /// Es nullo?
            if (existeMetodo is not null)
            {
                /// Esta deshabilitado?
                if (existeMetodo.EliminadoEn != null)
                    throw new NotFoundException("El metodo de pago existe, pero está deshabilitado.");

                throw new NotFoundException("El metodo de pago ya existe.");
            }

            var crearMetodo = new MetodoPago
            {
                Nombre = nombre,
                EliminadoEn = null
            };

            _context.MetodosPago.Add(crearMetodo);

            await _context.SaveChangesAsync();

            return crearMetodo.Id;

        }

        public async Task<List<MetodoPagoResponse>> ListarAsync()
        {
            //Consultamos => Trae el que no esta deshabilitado.
            var query = _context.MetodosPago.AsNoTracking().Where(m => m.EliminadoEn == null);

            //Selecciono lo que quiero mostrar.
            var listar = await query
                .Select(m => new MetodoPagoResponse
                (
                    m.Nombre
                )).ToListAsync();

            return listar;
        }
    }
}
