using GymManager.Shared.Contracts.Planes;

namespace GymManager.Api.Application.Interfaces
{
    public interface IPlanService
    {
        /// <summary>
        /// Listar Planes
        /// </summary>
        Task<List<PlanResponse>> ListarAsync();
        /// <summary>
        /// Crear Plan
        /// </summary>
        Task<int> CrearAsync(CreatePlanRequest request);
        /// <summary>
        /// Actualizar un plan
        /// </summary>
        Task UpdateAsync(int id, UpdatePlanRequest request);
        /// <summary>
        /// Detalles de un plan
        /// </summary>
        Task<PlanResponse> GetByIdAsync(int id);
        /// <summary>
        /// Eliminar logico
        /// </summary>
        Task SoftDeleteAsync(int id);
    }
}
