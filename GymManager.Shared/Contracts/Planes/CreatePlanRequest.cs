namespace GymManager.Shared.Contracts.Planes
{
    public record CreatePlanRequest(string Nombre, int? DuracionDias, decimal Precio);
}
