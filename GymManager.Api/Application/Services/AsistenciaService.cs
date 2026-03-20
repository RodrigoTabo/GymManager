using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Asistencias;
using GymManager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;

namespace GymManager.Api.Application.Services
{
    public class AsistenciaService(GymManagerDbContext context,
        ICurrentUserService currentUserService) : IAsistenciaService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<List<AsistenciaResponse>> ListarAsync(AsistenciaFiltro filtro)
        {

            var sucursalId = _currentUserService.SucursalIdOrThrow;

            var query = _context.Asistencias
                .AsNoTracking()
                .Where(a => a.SucursalId == sucursalId);

            //Filtramos por DNI
            if (!string.IsNullOrWhiteSpace(filtro.Dni))
                query = query.Where(a => a.Socio.DNI.Contains(filtro.Dni));

            //Filtramos por Nombre
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                query = query.Where(a =>
                a.Socio != null &&
                (a.Socio.Nombre + " " + a.Socio.Apellido).Contains(filtro.Nombre));

            //Filtramos por fecha desde
            if (filtro.Desde.HasValue)
                query = query.Where(a => a.FechaRegistro >= filtro.Desde.Value);

            //Filtramos por fecha hasta
            if (filtro.Hasta.HasValue)
                query = query.Where(a => a.FechaRegistro <= filtro.Hasta.Value);

            //Hacemos la lista y la ordenamso desde la mas nueva a la mas vieja.
            var listar = await query
                .OrderByDescending(a => a.FechaRegistro)
                .Select(a => new AsistenciaResponse
                {
                    FechaRegistro = a.FechaRegistro,
                    DNI = a.Socio.DNI,
                    Socio = a.Socio.Nombre + " " + a.Socio.Apellido,
                    Id = a.Id,
                    SocioId = a.SocioId,
                })
                .ToListAsync();

            return listar;

        }

        public async Task<MarcarAsistenciaResponse> MarcarPorDniAsync(string DNI)
        {

            var sucursalId = _currentUserService.SucursalIdOrThrow;

            var hoy = DateTime.Today;
            DateTime hoyInicio = DateTime.Today;
            var mañana = hoyInicio.AddDays(1);

            //Normalizamos el DNI
            var dniNormalizado = new string((DNI ?? "").Trim().Where(char.IsDigit).ToArray());

            if (string.IsNullOrWhiteSpace(dniNormalizado))
            {
                // Registramos intento (sin socio)
                _context.IntentosAccesos.Add(new IntentosAcceso
                {
                    FechaRegistro = DateTime.UtcNow,
                    DniIngresado = dniNormalizado,
                    SocioId = null,
                    Resultado = ResultadoAcceso.Denegada,
                    Motivo = MotivoAcceso.DniInvalido,
                    SucursalId = sucursalId
                });

                await _context.SaveChangesAsync();

                throw new BadRequestException("Ingresá un DNI válido (sin puntos).");
            }

            //Buscamos el socio
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.EliminadoEn == null && s.DNI == dniNormalizado && s.SucursalId == sucursalId);

            //Si el socio es nulo registramos que es nulo.
            if (socio is null)
            {
                _context.IntentosAccesos.Add(new IntentosAcceso
                {
                    FechaRegistro = DateTime.Now,
                    DniIngresado = dniNormalizado,
                    SocioId = null,
                    Resultado = ResultadoAcceso.Denegada,
                    Motivo = MotivoAcceso.SocioInexistente,
                    SucursalId = sucursalId
                });

                await _context.SaveChangesAsync();

                throw new NotFoundException("El socio no existe. Hable con el recepcionista.");
            }

            //Si el intento es valido, va como piña
            var intento = new IntentosAcceso
            {
                FechaRegistro = DateTime.UtcNow,
                DniIngresado = dniNormalizado,
                SocioId = socio.Id,
                Resultado = ResultadoAcceso.Aceptada,
                Motivo = MotivoAcceso.Ninguno,
                SucursalId = sucursalId
            };

            //Si el socio esta dado de baja, lo registramos que intento entrar un usuario dado de baja.
            if (socio.FechaBaja != null)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.SocioInactivo;
                intento.SucursalId = sucursalId;
                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Acceso denegado: socio inactivo.");
            }

            //Buscamos si tiene cuota vigente
            var ultimoPago = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.SocioId == socio.Id && p.SucursalId == sucursalId && p.EliminadoEn == null)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            var cuotaVigente = ultimoPago is not null && ultimoPago.CubreHasta.Date >= hoy.Date;
            //Si no tiene cuota vigente, lo registramos que intento y no tiene la cuota vigente.
            if (!cuotaVigente)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.CuotaVencida;
                intento.SucursalId = sucursalId;

                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Acceso denegado: no tenés una cuota vigente. Hablá con el recepcionista.");
            }

            var existeAsistencia = await _context.Asistencias
                .AnyAsync(a =>
                    a.Socio.DNI == dniNormalizado &&
                    a.SucursalId == sucursalId &&
                    a.FechaRegistro >= hoyInicio &&
                    a.FechaRegistro < mañana);

            if (existeAsistencia)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.YaMarcoHoy;
                intento.SucursalId = sucursalId;

                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Ya ha marcado el dia de hoy.");
            }

            //Guardamos la asistencia
            var asistencia = new Asistencia
            {
                FechaRegistro = DateTime.UtcNow,
                SocioId = socio.Id,
                SucursalId = sucursalId
            };

            //Guardo la transacción en la variable.
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.IntentosAccesos.Add(intento);
                _context.Asistencias.Add(asistencia);

                await _context.SaveChangesAsync();
                ///Confirmnamos la transacción
                await tx.CommitAsync();

            }
            catch
            {
                //Si no se pudo confirmar y se rompio en medio, volvemos para atrás.
                await tx.RollbackAsync();
                throw;
            }

            //Guardamos los intentos y las asistencias.

            return new MarcarAsistenciaResponse(
                asistencia.Id,
                socio.Nombre + " " + socio.Apellido,
                asistencia.FechaRegistro,
                "Bienvenido " + socio.Nombre + "!"
            );
        }
    }
}