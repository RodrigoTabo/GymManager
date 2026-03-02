using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infraestructure.Data;
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
            DateTime hoy = DateTime.Today;
            var Resultado = ResultadoAsistencia.Aceptada;
            var Motivo = MotivoAsistencia.Ninguno;

            var DNINormalizado = (DNI ?? "").Trim();

            //Validamos que exista DNI
            var socio = await _context.Socios.FirstOrDefaultAsync(s => s.EliminadoEn == null && s.DNI == DNINormalizado);

            if (socio is null)
                throw new NotFoundException("El Socio no existe. Hablá con el recepcionista.");

            if (socio.FechaBaja != null)
            {
                Resultado = ResultadoAsistencia.Denegada;
                Motivo = MotivoAsistencia.SocioInactivo;
            }
            else
            {
                var coutaVigente = await _context.Pagos.FirstOrDefaultAsync(p => p.SocioId == socio.Id && p.CubreDesde <= hoy && hoy <= p.CubreHasta);
                if (coutaVigente == null)
                {

                    Resultado = ResultadoAsistencia.Denegada;
                    Motivo = MotivoAsistencia.CuotaVencida;
                }
            }

            var asistencia = new Asistencia
            {
                FechaHora = DateTime.UtcNow,
                Resultado = Resultado,
                Motivo = Motivo,
                SocioId = socio.Id,
            };

            _context.Asistencias.Add(asistencia);

            await _context.SaveChangesAsync();


            return new MarcarAsistenciaResponse(
                    asistencia.Resultado,
                    asistencia.Motivo,
                    asistencia.SocioId,
                    asistencia.FechaHora,
                    socio.Nombre + " " + socio.Apellido,
                    null
                    );

        }
    }
}
