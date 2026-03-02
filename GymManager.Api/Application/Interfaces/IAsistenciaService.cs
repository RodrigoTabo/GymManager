using GymManager.Shared.Contracts.Asistencias;

namespace GymManager.Api.Application.Interfaces
{
    public interface IAsistenciaService
    {
        Task<MarcarAsistenciaResponse> MarcarPorDniAsync(string DNI);

    }
}
