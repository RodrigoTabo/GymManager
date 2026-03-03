using GymManager.Shared.Enums;


namespace GymManager.Shared.Contracts.Asistencias
{
    public record MarcarAsistenciaResponse(int Id, string? NombreCompleto, DateTime FechaRegistro, string Mensaje);
}
