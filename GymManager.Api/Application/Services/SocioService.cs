using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infraestructure.Data;
using GymManager.Shared.Contracts.Socios;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class SocioService : ISocioService
    {

        private readonly GymManagerDbContext _context;

        public SocioService(GymManagerDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearAsync(CreateSocioRequest request)
        {

            //Carga DNI
            var dni = (request.DNI ?? "").Trim();
            if (string.IsNullOrWhiteSpace(dni))
                throw new BadRequestException("El DNI es obligatorio");
            //Carga Nombre
            var nombre = (request.Nombre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BadRequestException("El nombre es obligatorio");
            //Carga Apellido
            var apellido = (request.Apellido ?? "").Trim();
            if (string.IsNullOrWhiteSpace(apellido))
                throw new BadRequestException("El apellido es obligatorio");

            //Existe? Existe Deshabilitado?
            var socio = await _context.Socios.AnyAsync(s=> s.DNI == request.DNI);
            if (socio)
                throw new NotFoundException("El socio ya existe.");

            //Existe? Existe Deshabilitado?
            var plan = await _context.Planes.FindAsync(request.PlanId);

            if (plan is null)
                throw new NotFoundException("Debes agregar un plan.");

            if (plan.EliminadoEn is not null)
                throw new ConflictException("El plan existe, pero está deshabilitado.");

            //Creamos socio
            var crearSocio = new Socio
            {
                DNI = dni,
                Nombre = nombre,
                Apellido = apellido,
                PlanId = request.PlanId,
                FechaNacimiento = request.FechaNacimiento,
                DocumentoId = request.documentoId,
                EliminadoEn = null
            };

            //Agregamos
            await _context.Socios.AddAsync(crearSocio);
            //Guardamos
            await _context.SaveChangesAsync();
            //Retornamos el socio
            return crearSocio.Id;
        }

        public async Task<SocioResponse> GetByIdAsync(int id)
        {
            //Optimizamos query, buscamos el socio, filtramos y pedimos los datos.
            var socio = await _context.Socios.AsNoTracking()
                .Where(s => s.Id == id && s.EliminadoEn == null)
                .Select(s => new SocioResponse
                (
                 s.DNI,
                 s.Nombre,
                 s.Apellido,
                 s.FechaNacimiento,
                 s.FechaAlta,
                 s.FechaBaja,
                 s.PlanId
                    ))
                .FirstOrDefaultAsync();

            //Validamos existencia.
            if (socio is null)
                throw new NotFoundException("El socio no existe.");
            //Retornamos el socio.
            return socio;
        }

        public async Task<List<SocioResponse>> ListarAsync()
        {
            //Optimizamos la query y filtramos.
            var query = _context.Socios.AsNoTracking();

            //Pedimos datos.
            var listar = await query
                .Where(s => s.EliminadoEn == null)
                .Select(s => new SocioResponse
                (
                    s.DNI,
                    s.Nombre,
                    s.Apellido,
                    s.FechaNacimiento,
                    s.FechaAlta,
                    s.FechaBaja,
                    s.PlanId
                    ))
                .ToListAsync();

            //Retornamos la lista.
            return listar;
        }

        public async Task SoftDeleteAsync(int id)
        {
            //Buscamos el socio
            var socio = await _context.Socios.FindAsync(id);

            //Existe Socio?
            if (socio is null)
                throw new NotFoundException("El socio no existe.");
            //Existe socio deshabilitado?
            if (socio.EliminadoEn != null)
                throw new ConflictException("El socio ya está deshabilitado");
            //Actualizamos
            socio.EliminadoEn = DateTime.UtcNow;
            //Impactamos Db.
            await _context.SaveChangesAsync();

        }

        public Task UpdateAsync(int id, UpdateSocioRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
