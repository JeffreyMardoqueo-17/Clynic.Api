using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IRolEspecialidadRepository
    {
        Task<RolEspecialidad?> ObtenerActivaAsync(int idRol, int idEspecialidad);
        Task<IReadOnlyCollection<RolEspecialidad>> ObtenerActivasPorRolAsync(int idRol);
        Task<RolEspecialidad> CrearAsync(RolEspecialidad rolEspecialidad);
    }
}
