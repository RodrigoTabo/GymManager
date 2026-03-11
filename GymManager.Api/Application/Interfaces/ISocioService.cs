using GymManager.Shared.Contracts.Socios;

namespace GymManager.Api.Application.Interfaces
{
    public interface ISocioService
    {
        /// <summary>
        /// Listar Socios
        /// </summary>
        Task<List<SocioResponse>> ListarAsync(Guid sucursalId, SocioQuery query);
        /// <summary>
        /// Crear Socio
        /// </summary>
        Task<int> CrearAsync(Guid sucursalId, CreateSocioRequest request);
        /// <summary>
        /// Actualizar Socio
        /// </summary>
        Task UpdateAsync(Guid sucursalId, int id, UpdateSocioRequest request);
        /// <summary>
        ///Mostrar socios por Id
        ///</summary>
        Task<SocioResponse> GetByIdAsync(Guid sucursalId, int id);
        /// <summary>
        /// Borrar logico
        /// </summary>
        Task SoftDeleteAsync(Guid sucursalId, int id);
    }
}
