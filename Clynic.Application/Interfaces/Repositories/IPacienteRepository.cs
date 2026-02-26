using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IPacienteRepository
    {
        Task<Paciente?> ObtenerPorIdAsync(int id);
        Task<Paciente?> ObtenerPorCorreoAsync(int idClinica, string correo);
        Task<IEnumerable<Paciente>> ObtenerPorClinicaAsync(int idClinica, string? busqueda = null);
        Task<Paciente> CrearAsync(Paciente paciente);
        Task<Paciente> ActualizarAsync(Paciente paciente);
        Task<HistorialClinico?> ObtenerHistorialPorPacienteAsync(int idPaciente);
        Task<HistorialClinico> GuardarHistorialAsync(HistorialClinico historial);
    }
}
