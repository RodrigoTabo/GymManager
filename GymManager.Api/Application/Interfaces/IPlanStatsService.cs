using GymManager.Shared.Contracts.Planes;

namespace GymManager.Api.Application.Interfaces
{
    public interface IPlanStatsService
    {
        /// <summary>
        /// Stats planes
        /// </summary>
        Task<StatsPlanRequest> GetStatsAsync();
    }
}
