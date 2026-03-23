using Azure.Core;
using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Socios;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class SocioService(GymManagerDbContext context, ICurrentUserService currentUserService) : ISocioService
    {

        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<int> CrearAsync(CreateSocioRequest request)
        {
            //Traemos la sucursalId para comparar.
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Llamamos al metodo que valida si ya existe un socio con ese DNI.
            await ValidarSocioUnicoAsync(request.DNI, sucursalId);

            //Llamamos al metodo que valida si existe un plan
            await ValidarPlanAsync(request.PlanId, sucursalId);

            //Creamos socio
            var crearSocio = new Socio
            {
                DNI = request.DNI,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Telefono = request.Telefono,
                PlanId = request.PlanId,
                FechaAlta = DateTime.UtcNow,
                FechaNacimiento = request.FechaNacimiento,
                DocumentoId = request.documentoId,
                EliminadoEn = null,
                SucursalId = sucursalId
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

            //Traemos la sucursal para comparar.
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            // Traemos el socio ya filtrando eliminado y sucursal
            var socio = await _context.Socios
                .AsNoTracking()
                .Where(s => s.Id == id && s.EliminadoEn == null && s.SucursalId == sucursalId)
                .Select(s => new SocioResponse(
                    s.Id,
                    s.DNI,
                    s.Nombre,
                    s.Apellido,
                    s.Telefono,
                    s.FechaNacimiento,
                    s.FechaAlta,
                    s.FechaBaja,
                    s.PlanId,
                    s.Plan.Nombre
                ))
                .FirstOrDefaultAsync();

            //Si el socio no existe...
            if (socio is null)
                throw new NotFoundException("El socio no existe.");

            //Retornamos el objeto.
            return socio;
        }

        public async Task<List<SocioResponse>> ListarAsync(SocioQuery query)
        {

            //Traemos la sucursalId para comparar
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Optimizamos la query y filtramos.
            var consulta = _context.Socios
                .AsNoTracking()
                .Where(s => s.SucursalId == sucursalId)
                .Where(s => query.Inactivo ? s.EliminadoEn != null : s.EliminadoEn == null);

            //Normalizamos el texto.
            var texto = (query.Texto ?? "").Trim();

            //Utilizamos el switch para filtrar las opciones que selecciono el usuario.
            if (!string.IsNullOrWhiteSpace(texto))
            {
                consulta = query.BuscarPor switch
                {
                    "DNI" => consulta.Where(s => s.DNI.Contains(texto)),
                    "NombreCompleto" => consulta.Where(s => (s.Nombre + " " + s.Apellido).Contains(texto)),
                    "Plan" => consulta.Where(s => s.Plan.Nombre.Contains(texto)),
                    _ => consulta
                };
            }
            //Pedimos datos.
            var listar = await consulta
                .OrderByDescending(s => s.FechaAlta)
                .Select(s => new SocioResponse
                (
                    s.Id,
                    s.DNI,
                    s.Nombre,
                    s.Apellido,
                    s.Telefono,
                    s.FechaNacimiento,
                    s.FechaAlta,
                    s.FechaBaja,
                    s.PlanId,
                    s.Plan.Nombre
                    ))
                .ToListAsync();

            //Retornamos la lista.
            return listar;
        }

        public async Task SoftDeleteAsync(int id)
        {
            //Traemos la sucursalId para comparar.
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Optimizamos la consulta y filtramos el socio.
            var socio = await ObtenerSocioActivoAsync(id, sucursalId);

            socio.EliminadoEn = DateTime.UtcNow;

            //Guardamos los cambios.
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, UpdateSocioRequest request)
        {

            //Traemos la sucursalId para comparar.
            var sucursalId = _currentUserService.SucursalIdOrThrow;

            //Llamamos al metodo de validacion de socio
            var socio = await ObtenerSocioActivoAsync(id, sucursalId);

            //Buscamos si el DNI registrado ya existe.
            var socioExiste = await _context.Socios.AnyAsync(s => s.DNI == request.DNI && s.Id != id && s.SucursalId == sucursalId);

            //El socio existe?
            if (socioExiste)
                throw new ConflictException("El Socio ya existe.");

            //Buscamos el plan mediante Id
            await ValidarPlanAsync(request.PlanId, sucursalId);

            socio.DNI = request.DNI;
            socio.Nombre = request.Nombre;
            socio.Apellido = request.Apellido;
            socio.Telefono = request.Telefono;
            socio.FechaNacimiento = request.FechaNacimiento;
            socio.PlanId = request.PlanId;
            socio.DocumentoId = request.documentoId;

            //Guardamos los cambios.
            await _context.SaveChangesAsync();

        }

        //METODOS PRIVADOS

        private async Task<Socio> ObtenerSocioActivoAsync(int id, Guid sucursalId)
        {
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.Id == id && s.SucursalId == sucursalId && s.EliminadoEn == null);

            if (socio == null)
                throw new NotFoundException("El socio no existe.");

            return socio;
        }

        // Valida que no exista un socio con el mismo DNI
        private async Task ValidarSocioUnicoAsync(string dni, Guid sucursalId)
        {
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.DNI == dni && s.SucursalId == sucursalId);

            if (socio != null)
            {
                if (socio.EliminadoEn != null)
                    throw new ConflictException("El Socio ya existe pero está eliminado.");

                throw new ConflictException("El Socio ya existe.");
            }
        }

        private async Task ValidarPlanAsync(int id, Guid sucursalId)
        {
            // Busamos si existe un plan con ese Id en la sucursal
            var plan = await _context.Planes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.SucursalId == sucursalId);

            //Si no existe...
            if (plan == null)
                throw new ConflictException("Debes elegir un plan.");

            //Esta eliminado?
            if (plan.EliminadoEn != null)
                throw new ConflictException("El Plan ya existe pero está eliminado.");
        }

    }
}
