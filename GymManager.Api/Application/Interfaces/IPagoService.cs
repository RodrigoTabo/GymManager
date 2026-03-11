using GymManager.Shared.Contracts.Pagos;

namespace GymManager.Api.Application.Interfaces
{
    public interface IPagoService
    {
        /// <summary>
        /// Listamos los pagos
        /// </summary>
        /// <returns></returns>
        Task<List<PagoResponse>> ListarAsync(Guid sucursalId);
        /// <summary>
        /// Crear nuevo Pago
        /// </summary>
        Task<int> CrearAsync(Guid sucursalId, CreatePagoRequest request);
        /// <summary>
        /// Editar un Pago
        /// </summary>
        Task UpdateAsync(Guid sucursalId, UpdatePagoRequest request, int id);

        /// <summary>
        /// Eliminacion logica 
        /// </summary>
        Task SoftDeleteAsync(Guid sucursalId, int id);
    }
}
