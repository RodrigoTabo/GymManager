using GymManager.Shared.Contracts.Planes;

namespace GymManager.Api.Application.Interfaces
{
    public interface IPlanService
    {
        Task<List<PlanResponse>> ListarAsync();

        Task<int> CrearAsync(CreatePlanRequest request);

        Task UpdateAsync(int id, UpdatePlanRequest request);
    }
}
