using GymManager.Api.Application.Interfaces;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.IntentosAcceso;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class IntentosAccessoService(GymManagerDbContext context) : IIntentosAccesoService
    {
        private readonly GymManagerDbContext _context = context;

        public async Task<List<IntentosAccesoResponse>> ListarAsync(IntentosAccesoFiltro filtro)
        {
            //armo la query
            var query = _context.IntentosAccesos
                .Include(s => s.Socio)
                .AsNoTracking();
            //Por DNI
            if (!string.IsNullOrWhiteSpace(filtro.Dni))
                query = query.Where(i => i.DniIngresado.Contains(filtro.Dni));
            //Por nombre
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                query = query.Where(i =>
                    i.Socio != null &&
                    (i.Socio.Nombre + " " + i.Socio.Apellido).Contains(filtro.Nombre));
            //Por resultados
            if (filtro.Resultado.HasValue)
                query = query.Where(i => i.Resultado == filtro.Resultado.Value);
            //Por Motivos
            if (filtro.Motivo.HasValue)
                query = query.Where(i => i.Motivo == filtro.Motivo.Value);
            //Desde
            if (filtro.Desde.HasValue)
                query = query.Where(i => i.FechaRegistro >= filtro.Desde.Value);
            //Hasta
            if (filtro.Hasta.HasValue)
                query = query.Where(i => i.FechaRegistro <= filtro.Hasta.Value);
            //Lista
            var listar = await query
                .Select(i => new IntentosAccesoResponse
                {
                    FechaRegistro = i.FechaRegistro,
                    DniIngresado = i.DniIngresado,
                    SocioId = i.SocioId,
                    Socio = i.Socio.Nombre + " " + i.Socio.Apellido,
                    Resultado = i.Resultado.ToString(),
                    Motivo = i.Motivo.ToString()
                })
                .ToListAsync();

            return listar;

        }

    }
}