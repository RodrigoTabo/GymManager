using GymManager.Shared.Contracts.General;

namespace GymManager.Api.Application.Interfaces
{
    public interface IGeneralService
    {

        Task<GeneralResponse> GetStatsGeneralAsync();

    }
}
