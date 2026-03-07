using GymManager.Shared.Contracts.MetodoPago;

namespace GymManager.Api.Application.Interfaces
{
    public interface IMetodoPagoService
    {


        /// <summary>
        /// Traer lista de metodos pago
        /// </summary>
        Task<List<MetodoPagoResponse>> ListarAsync();
        /// <summary>
        /// Crear Metodo de Pago
        /// </summary>
        Task<int> CrearAsync(CreateMetodoPagoRequest request);
        /// <summary>
        /// Updateamos
        /// </summary>
        Task UpdateAsync (UpdateMetodoPagoRequest request, int d);
        /// <summary>
        /// Deleteamos
        /// </summary>
        Task SoftDeleteAsync(int id);


    }
}
