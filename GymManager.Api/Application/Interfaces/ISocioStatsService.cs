using GymManager.Shared.Contracts.Socios;

namespace GymManager.Api.Application.Interfaces
{
    public interface ISocioStatsService
    {
        /// <summary>
        /// Stats Socios
        /// </summary>
        Task<SociosStatsResponse> GetStatsAsync();
    }
}
