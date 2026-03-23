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

            //Registramos DNI Invalido
            await RegistrarDniInvalidoAsync(dniNormalizado, sucursalId);

            //Registramos socio Inexistente
            var socio = await RegistrarSocioInexistenteAsync(dniNormalizado, sucursalId);

            //Guardamos ValidarIntento
            var intento = CrearIntentoExitoso(dniNormalizado, socio.Id, sucursalId);

            //Guardamos socio dado de baja
            await RegistrarSocioBajaAsync(socio, intento.Result, sucursalId);

            //Obtenemos ultimo pago
            var ultimoPago = await ObtenerUltimoPago(socio.Id, sucursalId);

            //Registramos que no tiene couota vigente.
            await RegistrarCoutaVencidaAsync(ultimoPago, sucursalId, hoy, intento.Result);

            //Validamos que no haya marcado hoy
            var existeAsistencia = await ObtenerAsistencia(dniNormalizado, sucursalId, hoy, mañana);

            await RegistrarAsistenciaExistenteAsync(existeAsistencia, intento.Result, sucursalId);

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
                _context.IntentosAccesos.Add(intento.Result);
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


        //METODOS PRIVADOS

        private async Task RegistrarDniInvalidoAsync(string DNI, Guid sucursalId)
        {
            //Si el DNI es invalido, lo registramos como invalido, y lanzamos la excepcion.
            if (string.IsNullOrWhiteSpace(DNI))
            {
                _context.IntentosAccesos.Add(new IntentosAcceso
                {
                    FechaRegistro = DateTime.UtcNow,
                    DniIngresado = DNI,
                    SocioId = null,
                    Resultado = ResultadoAcceso.Denegada,
                    Motivo = MotivoAcceso.DniInvalido,
                    SucursalId = sucursalId
                });

                await _context.SaveChangesAsync();

                throw new BadRequestException("Ingresá un DNI válido (sin puntos).");
            }
        }

        private async Task<Socio> RegistrarSocioInexistenteAsync(string DNI, Guid sucursalId)
        {
            //Buscamos el socio
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.DNI == DNI && s.SucursalId == sucursalId);

            //Si el socio es nulo registramos que es nulo y lanzamos la excepcion
            if (socio is null)
            {
                _context.IntentosAccesos.Add(new IntentosAcceso
                {
                    FechaRegistro = DateTime.Now,
                    DniIngresado = DNI,
                    SocioId = null,
                    Resultado = ResultadoAcceso.Denegada,
                    Motivo = MotivoAcceso.SocioInexistente,
                    SucursalId = sucursalId
                });

                await _context.SaveChangesAsync();

                throw new NotFoundException("El socio no existe. Hable con el recepcionista.");
            }
            return socio;
        }

        private async Task<IntentosAcceso> CrearIntentoExitoso(string DNI, int id, Guid sucursalId)
        {
            //Guardamos la asistencia exitosa.
            var intento = new IntentosAcceso
            {
                FechaRegistro = DateTime.UtcNow,
                DniIngresado = DNI,
                SocioId = id,
                Resultado = ResultadoAcceso.Aceptada,
                Motivo = MotivoAcceso.Ninguno,
                SucursalId = sucursalId
            };

            return intento;

        }

        private async Task RegistrarSocioBajaAsync(Socio socio, IntentosAcceso intento, Guid sucursalId)
        {
            //Si el socio esta dado de baja, lo registramos que intento entrar un usuario dado de baja.
            if (socio.EliminadoEn != null)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.SocioInactivo;
                intento.SucursalId = sucursalId;
                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Acceso denegado: socio inactivo.");
            }

        }

        private async Task<Pago> ObtenerUltimoPago(int socio, Guid sucursalId)
        {
            //Buscamos si tiene cuota vigente
            var ultimoPago = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.SocioId == socio && p.SucursalId == sucursalId && p.EliminadoEn == null)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            return ultimoPago;
        }

        private async Task RegistrarCoutaVencidaAsync(Pago ultimoPago, Guid sucursalId, DateTime hoy, IntentosAcceso intento)
        {
            //Si el ultimo pago existe, pero esta vencida...
            var cuotaVigente = ultimoPago is not null && ultimoPago.CubreHasta.Date >= hoy.Date;
         
            if (!cuotaVigente)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.CuotaVencida;
                intento.SucursalId = sucursalId;

                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Acceso denegado: no tenés una cuota vigente. Hablá con el recepcionista.");
            }
        }

        private async Task<bool> ObtenerAsistencia(string DNI, Guid sucursalId, DateTime hoyInicio, DateTime mañana)
        {
            //Buscamos si este usuario, ya tiene una asistencia el dia de hoy...
            var existeAsistencia = await _context.Asistencias
                .AnyAsync(a =>
                    a.Socio.DNI == DNI &&
                    a.SucursalId == sucursalId &&
                    a.FechaRegistro >= hoyInicio &&
                    a.FechaRegistro < mañana);

            return existeAsistencia;

        }

        private async Task RegistrarAsistenciaExistenteAsync(bool existeAsistencia, IntentosAcceso intento, Guid sucursalId)
        {
            //Si existe la asistencia, entonces ya marco y lanzamos la excepcion correspondiente.
            if (existeAsistencia)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.YaMarcoHoy;
                intento.SucursalId = sucursalId;

                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Ya ha marcado el dia de hoy.");
            }
        }
    }
}