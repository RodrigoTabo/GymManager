using GymManager.Shared.Contracts.MetodoPago;

namespace GymManager.Api.Application.Interfaces
{
    public interface IMetodoPago
    {

        /// <summary>
        /// Crear Metodo de Pago
        /// </summary>
        Task<int> CrearAsync(CreateMetodoPagoRequest request);
    }
}
