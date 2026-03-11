using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IEspecialidadRepository
    {
        Task<Especialidad?> ObtenerPorIdAsync(int id);
        Task<Especialidad?> ObtenerPorNombreAsync(int idClinica, string nombre);
        Task<IReadOnlyCollection<Especialidad>> ObtenerActivasAsync(int idClinica);
        Task<bool> ExisteActivaAsync(int idClinica, int id);
        Task<Especialidad> CrearAsync(Especialidad especialidad);
    }
}
