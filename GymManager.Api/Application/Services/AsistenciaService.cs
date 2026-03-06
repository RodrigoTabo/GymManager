using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.Asistencias;
using GymManager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using static GymManager.Api.Application.Middleware.ApiExceptionHandling;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GymManager.Api.Application.Services
{
    public class AsistenciaService(GymManagerDbContext context) : IAsistenciaService
    {
        private readonly GymManagerDbContext _context = context;

        public async Task<List<AsistenciaResponse>> ListarAsync(AsistenciaFiltro filtro)
        {
            var query = _context.Asistencias.Include(s => s.Socio).AsNoTracking();

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

            //Hacemos la lista
            var listar = await query
                .Select(a => new AsistenciaResponse
                {
                    FechaRegistro = a.FechaRegistro,
                    Socio = a.Socio.Nombre + " " + a.Socio.Apellido,
                    Id = a.Id,
                    SocioId = a.SocioId,
                }).ToListAsync();

            return listar;

        }

        public async Task<MarcarAsistenciaResponse> MarcarPorDniAsync(string DNI)
        {
            var hoy = DateTime.UtcNow;
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
                    Motivo = MotivoAcceso.DniInvalido
                });

                await _context.SaveChangesAsync();

                throw new BadRequestException("Ingresá un DNI válido (sin puntos).");
            }
            //Buscamos el socio
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.EliminadoEn == null && s.DNI == dniNormalizado);
            //Si el socio es nulo registramos que es nulo.
            if (socio is null)
            {
                _context.IntentosAccesos.Add(new IntentosAcceso
                {
                    FechaRegistro = DateTime.UtcNow,
                    DniIngresado = dniNormalizado,
                    SocioId = null,
                    Resultado = ResultadoAcceso.Denegada,
                    Motivo = MotivoAcceso.SocioInexistente
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
                Motivo = MotivoAcceso.Ninguno
            };
            //Si el socio esta dado de baja, lo registramos que intento entrar un usuario dado de baja.
            if (socio.FechaBaja != null)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.SocioInactivo;

                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Acceso denegado: socio inactivo.");
            }
            //Buscamos si tiene cuota vigente
            var cuotaVigente = await _context.Pagos
                .AnyAsync(p => p.SocioId == socio.Id && p.CubreDesde <= hoy && hoy <= p.CubreHasta);
            //Si no tiene cuota vigente, lo registramos que intento y no tiene la cuota vigente.
            if (!cuotaVigente)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.CuotaVencida;

                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Acceso denegado: no tenés una cuota vigente.");
            }
            //Guardamos la asistencia
            var asistencia = new Asistencia
            {
                FechaRegistro = DateTime.UtcNow,
                SocioId = socio.Id
            };

            //Guardamos los intentos y las asistencias.
            _context.IntentosAccesos.Add(intento);
            _context.Asistencias.Add(asistencia);

            await _context.SaveChangesAsync();

            return new MarcarAsistenciaResponse(
                asistencia.Id,
                socio.Nombre + " " + socio.Apellido,
                asistencia.FechaRegistro,
                "Bienvenido " + socio.Nombre + "!"
            );
        }
    }
}