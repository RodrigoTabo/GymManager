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

        public async Task<MarcarAsistenciaResponse> MarcarPorDniAsync(string DNI)
        {
            var hoy = DateTime.UtcNow;
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

            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.EliminadoEn == null && s.DNI == dniNormalizado);

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

            var intento = new IntentosAcceso
            {
                FechaRegistro = DateTime.UtcNow,
                DniIngresado = dniNormalizado,
                SocioId = socio.Id,
                Resultado = ResultadoAcceso.Aceptada,
                Motivo = MotivoAcceso.Ninguno
            };

            if (socio.FechaBaja != null)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.SocioInactivo;

                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Acceso denegado: socio inactivo.");
            }

            var cuotaVigente = await _context.Pagos
                .AnyAsync(p => p.SocioId == socio.Id && p.CubreDesde <= hoy && hoy <= p.CubreHasta);

            if (!cuotaVigente)
            {
                intento.Resultado = ResultadoAcceso.Denegada;
                intento.Motivo = MotivoAcceso.CuotaVencida;

                _context.IntentosAccesos.Add(intento);
                await _context.SaveChangesAsync();

                throw new ConflictException("Acceso denegado: no tenés una cuota vigente.");
            }

            var asistencia = new Asistencia
            {
                FechaRegistro = DateTime.UtcNow,
                SocioId = socio.Id
            };

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