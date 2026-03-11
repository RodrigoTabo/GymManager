using GymManager.Shared.Contracts.Planes;

namespace GymManager.Api.Application.Interfaces
{
    public interface IPlanService
    {
        /// <summary>
        /// Listar Planes
        /// </summary>
        Task<List<PlanResponse>> ListarAsync(Guid sucursalId);
        /// <summary>
        /// Crear Plan
        /// </summary>
        Task<int> CrearAsync(Guid sucursalId, CreatePlanRequest request);
        /// <summary>
        /// Actualizar un plan
        /// </summary>
        Task UpdateAsync(Guid sucursalId, int id, UpdatePlanRequest request);
        /// <summary>
        /// Detalles de un plan
        /// </summary>
        Task<PlanResponse> GetByIdAsync(Guid sucursalId, int id);
        /// <summary>
        /// Eliminar logico
        /// </summary>
        Task SoftDeleteAsync(Guid sucursalId, int id);
    }
}
