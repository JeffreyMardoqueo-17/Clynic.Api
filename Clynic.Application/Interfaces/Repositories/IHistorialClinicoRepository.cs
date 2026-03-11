using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IHistorialClinicoRepository
    {
        Task<HistorialClinico?> ObtenerPorPacienteAsync(int idPaciente);
        Task<HistorialClinico> GuardarAsync(HistorialClinico historialClinico);
    }
}
