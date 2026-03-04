using GymManager.Shared.Contracts.Socios;

namespace GymManager.Api.Application.Interfaces
{
    public interface ISocioService
    {
        /// <summary>
        /// Listar Socios
        /// </summary>
        Task<List<SocioResponse>> ListarAsync(SocioQuery query);
        /// <summary>
        /// Crear Socio
        /// </summary>
        Task<int> CrearAsync(CreateSocioRequest request);
        /// <summary>
        /// Actualizar Socio
        /// </summary>
        Task UpdateAsync(int id, UpdateSocioRequest request);
        /// <summary>
        ///Mostrar socios por Id
        ///</summary>
        Task<SocioResponse> GetByIdAsync(int id);
        /// <summary>
        /// Borrar logico
        /// </summary>
        Task SoftDeleteAsync(int id);
        /// <summary>
        /// Stats Socios
        /// </summary>
        Task<SociosStatsResponse> GetStatsAsync();
    }
}
