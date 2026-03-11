using GymManager.Shared.Contracts.IntentosAcceso;

namespace GymManager.Api.Application.Interfaces
{
    public interface IIntentosAccesoService
    {
        Task<List<IntentosAccesoResponse>> ListarAsync(Guid sucursalId, IntentosAccesoFiltro filtro);
    }
}