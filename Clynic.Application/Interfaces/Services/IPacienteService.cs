using Clynic.Application.DTOs.Pacientes;

namespace Clynic.Application.Interfaces.Services
{
    public interface IPacienteService
    {
        Task<IEnumerable<PacienteResponseDto>> ObtenerPorClinicaAsync(int idClinica, string? busqueda = null);
        Task<PacienteResponseDto?> ObtenerPorIdAsync(int idPaciente);
        Task<PacienteResponseDto?> ActualizarAsync(int idPaciente, UpdatePacienteDto dto);
        Task<HistorialClinicoResponseDto> GuardarHistorialAsync(int idPaciente, UpdateHistorialClinicoDto dto);
    }
}
