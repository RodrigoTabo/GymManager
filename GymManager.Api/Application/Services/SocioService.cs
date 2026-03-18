using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Socios;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class SocioService(GymManagerDbContext context,
        ISucursalAccessValidator sucursalAccessValidator) : ISocioService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ISucursalAccessValidator _sucursalAccessValidator = sucursalAccessValidator;

        public async Task<int> CrearAsync(Guid sucursalid, CreateSocioRequest request)
        {

            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Carga DNI
            var dni = (request.DNI ?? "").Trim();
            if (string.IsNullOrWhiteSpace(dni))
                throw new BadRequestException("El DNI es obligatorio");
            //Carga Nombre
            var nombre = (request.Nombre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BadRequestException("El Nombre es obligatorio");
            //Carga Apellido
            var apellido = (request.Apellido ?? "").Trim();
            if (string.IsNullOrWhiteSpace(apellido))
                throw new BadRequestException("El Apellido es obligatorio");

            //Existe? Existe Deshabilitado?
            var socio = await _context.Socios
                .AnyAsync(s => s.DNI == dni && s.SucursalId == sucursalId);

            if (socio)
                throw new ConflictException("El Socio ya existe.");

            //Existe? Existe Deshabilitado?
            var plan = await _context.Planes.FindAsync(request.PlanId);
            
            if (plan is null)
                throw new NotFoundException("Debes agregar un Plan.");

            if (plan.EliminadoEn is not null)
                throw new ConflictException("El plan existe, pero está deshabilitado.");

            if(plan.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Creamos socio
            var crearSocio = new Socio
            {
                DNI = dni,
                Nombre = nombre,
                Apellido = apellido,
                Telefono = request.Telefono,
                PlanId = request.PlanId,
                FechaAlta= DateTime.UtcNow,
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

        public async Task<SocioResponse> GetByIdAsync(Guid sucursalid, int id)
        {
            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Optimizamos query, buscamos el socio, filtramos y pedimos los datos.
            var socio = await _context.Socios.AsNoTracking()
                .Where(s => s.Id == id && s.EliminadoEn == null && s.SucursalId == sucursalId)
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
                .FirstOrDefaultAsync();

            //Validamos existencia.
            if (socio is null)
                throw new NotFoundException("El Socio no existe.");
            //Retornamos el socio.
            return socio;
        }

        public async Task<List<SocioResponse>> ListarAsync(Guid sucursalid, SocioQuery query)
        {
            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Optimizamos la query y filtramos.
            var consulta = _context.Socios.AsNoTracking().Where(s => s.SucursalId == sucursalId);

            consulta = query.Inactivo
                ? consulta.Where(s => s.EliminadoEn != null)   // solo inactivos
                : consulta.Where(s => s.EliminadoEn == null);  // solo activos

            var texto = (query.Texto ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                if (query.BuscarPor == "DNI")
                    consulta = consulta.Where(s => s.DNI.Contains(texto));
                else if (query.BuscarPor == "NombreCompleto")
                    consulta = consulta.Where(s => (s.Nombre + " " + s.Apellido).Contains(texto));
                else
                    consulta = consulta.Where(s => s.Plan.Nombre.Contains(texto));
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

        public async Task SoftDeleteAsync(Guid sucursalid, int id)
        {
            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Buscamos el socio
            var socio = await _context.Socios.FindAsync(id);

            //Existe Socio?
            if (socio is null)
                throw new NotFoundException("El socio no existe.");
            //Existe socio deshabilitado?
            if (socio.EliminadoEn != null)
                throw new ConflictException("El socio ya está deshabilitado");
            if(socio.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Actualizamos
            socio.EliminadoEn = DateTime.UtcNow;
            //Impactamos Db.
            await _context.SaveChangesAsync();

        }

        public async Task UpdateAsync(Guid sucursalid, int id, UpdateSocioRequest request)
        {
            var sucursalId = await _sucursalAccessValidator.ValidarYObtenerSucursalAsync(sucursalid);

            if (sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            var socio = await _context.Socios.FindAsync(id);

            if (socio is null)
                throw new NotFoundException("El Socio que desea modificar no existe.");

            if (socio.EliminadoEn != null)
                throw new ConflictException("El Socio está deshabilitado.");

            if (socio.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            //Carga DNI
            var dni = (request.DNI ?? "").Trim();
            if (string.IsNullOrWhiteSpace(dni))
                throw new BadRequestException("El DNI es obligatorio");
            //Carga Nombre
            var nombre = (request.Nombre ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
                throw new BadRequestException("El Nombre es obligatorio");
            //Carga Apellido
            var apellido = (request.Apellido ?? "").Trim();
            if (string.IsNullOrWhiteSpace(apellido))
                throw new BadRequestException("El Apellido es obligatorio");

            //Existe? Existe Deshabilitado?
            var socioExiste = await _context.Socios.AnyAsync(s => s.DNI == dni && s.Id != id && s.SucursalId == sucursalId);
            if (socioExiste)
                throw new ConflictException("El Socio ya existe.");

            //Existe? Existe Deshabilitado?
            var plan = await _context.Planes.FindAsync(request.PlanId);

            if (plan is null)
                throw new NotFoundException("Debes agregar un Plan.");

            if (plan.EliminadoEn is not null)
                throw new ConflictException("El plan existe, pero está deshabilitado.");

            if (plan.SucursalId != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada, no coincide con la sucursal activa.");

            socio.DNI = dni;
            socio.Nombre = nombre;
            socio.Apellido = apellido;
            socio.Telefono = request.Telefono;
            socio.FechaNacimiento = request.FechaNacimiento;
            socio.PlanId = request.PlanId;
            socio.DocumentoId = request.documentoId;

            await _context.SaveChangesAsync();

        }
    }
}
