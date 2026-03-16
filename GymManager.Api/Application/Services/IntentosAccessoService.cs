using GymManager.Api.Application.Interfaces;
using GymManager.Api.Domain.Entities;
using GymManager.Api.Infrastructure.Data;
using GymManager.Shared.Contracts.IntentosAcceso;
using Microsoft.EntityFrameworkCore;

namespace GymManager.Api.Application.Services
{
    public class IntentosAccessoService(GymManagerDbContext context,
        ICurrentSucursalService currentSucursalService,
        ICurrentUserService currentUserService) : IIntentosAccesoService
    {
        private readonly GymManagerDbContext _context = context;
        private readonly ICurrentSucursalService _currentSucursalService = currentSucursalService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        public async Task<List<IntentosAccesoResponse>> ListarAsync(Guid sucursalid, IntentosAccesoFiltro filtro)
        {
            
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

            var sucursalId = _currentSucursalService.SucursalId
                ?? throw new UnauthorizedAccessException("Sucursal no informada.");

            if(sucursalid != sucursalId)
                throw new UnauthorizedAccessException("La sucursal solicitada no coincide con la sucursal activa.");

            var autorizado = await _context.UsuarioSucursales
                .AnyAsync(x => x.UsuarioId == userId && x.SucursalId == sucursalId);

            if (!autorizado)
                throw new UnauthorizedAccessException("No tenés acceso a esta sucursal.");

            //armo la query
            var query = _context.IntentosAccesos
                .AsNoTracking()
                .Where(s=> s.SucursalId == sucursalId);

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

            //Lista y ordenamos por registro mas nuevo al mas viejo.
            var listar = await query
                .OrderByDescending(i => i.FechaRegistro)
                .Select(i => new IntentosAccesoResponse
                {
                    FechaRegistro = i.FechaRegistro,
                    DniIngresado = i.DniIngresado,
                    SocioId = i.SocioId,
                    Socio = i.Socio == null ? "—" : i.Socio.Nombre + " " + i.Socio.Apellido,
                    Resultado = i.Resultado.ToString(),
                    Motivo = i.Motivo.ToString()
                })
                .ToListAsync();

            return listar;

        }

    }
}