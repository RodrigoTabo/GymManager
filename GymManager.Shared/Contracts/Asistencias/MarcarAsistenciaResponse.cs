using GymManager.Shared.Enums;


namespace GymManager.Shared.Contracts.Asistencias
{
    public record MarcarAsistenciaResponse(ResultadoAsistencia resultado, MotivoAsistencia motivo, int socioId, DateTime FechaHora, string? NombreCompleto, string? PlanNombre);
}
