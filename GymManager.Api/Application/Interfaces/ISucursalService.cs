using GymManager.Shared.Contracts.Sucursal;

namespace GymManager.Api.Application.Interfaces
{
    public interface ISucursalService
    {
        Task<List<SucursalResponse>> GetSucursalAsync();
    }
}
