using GymManager.Shared.Contracts.Pagos;

namespace GymManager.Api.Application.Interfaces
{
    public interface IPagoStatsService
    {
        /// <summary>
        /// Stats Pagos
        /// </summary>
        Task<PagosStatsResponse> GetStatsAsync(Guid sucursalId);
    }
}
