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


    }
}
