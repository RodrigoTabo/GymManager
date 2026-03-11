using GymManager.Api.Application.Services;
using GymManager.Shared.Contracts.Asistencias;

namespace GymManager.Api.Application.Interfaces
{
    public interface IAsistenciaService
    {
        /// <summary>
        /// Registrar Asistencia/Intentos Accesos
        /// </summary>
        Task<MarcarAsistenciaResponse> MarcarPorDniAsync(Guid sucursalId, string DNI);
        /// <summary>
        /// Listar Asistencias
        /// </summary>
        Task<List<AsistenciaResponse>> ListarAsync(Guid sucursalId, AsistenciaFiltro filtro);
    }
}
