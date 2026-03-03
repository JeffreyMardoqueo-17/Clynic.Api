using Clynic.Application.DTOs.HistorialesClinicos;

namespace Clynic.Application.Interfaces.Services
{
    public interface IHistorialClinicoService
    {
        Task<HistorialClinicoResponseDto?> ObtenerPorPacienteAsync(int idPaciente);
        Task<HistorialClinicoResponseDto> GuardarAsync(int idPaciente, UpsertHistorialClinicoDto dto);
    }
}
