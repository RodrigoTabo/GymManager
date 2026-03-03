using GymManager.Shared.Contracts.Asistencias;

namespace GymManager.Api.Application.Interfaces
{
    public interface IAsistenciaService
    {
        /// <summary>
        /// Registrar Asistencia/Intentos Accesos
        /// </summary>
        Task<MarcarAsistenciaResponse> MarcarPorDniAsync(string DNI);

    }
}
