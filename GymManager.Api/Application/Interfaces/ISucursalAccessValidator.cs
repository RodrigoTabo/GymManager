namespace GymManager.Api.Application.Interfaces
{
    public interface ISucursalAccessValidator
    {
        Task<Guid> ValidarYObtenerSucursalAsync(Guid sucursalIdParam);
    }
}
