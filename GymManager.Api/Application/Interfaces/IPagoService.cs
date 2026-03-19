using GymManager.Shared.Contracts.Pagos;

namespace GymManager.Api.Application.Interfaces
{
    public interface IPagoService
    {
        /// <summary>
        /// Listamos los pagos
        /// </summary>
        /// <returns></returns>
        Task<List<PagoResponse>> ListarAsync();
        /// <summary>
        /// Crear nuevo Pago
        /// </summary>
        Task<int> CrearAsync(CreatePagoRequest request);
        /// <summary>
        /// Editar un Pago
        /// </summary>
        Task UpdateAsync(UpdatePagoRequest request, int id);

        /// <summary>
        /// Eliminacion logica 
        /// </summary>
        Task SoftDeleteAsync(int id);
        /// <summary>
        /// Listar vencidos
        /// </summary>
        Task<List<VencidoResponse>> GetVencidosAsync();
    }
}
