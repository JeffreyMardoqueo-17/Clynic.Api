using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface ISucursalEspecialidadRepository
    {
        Task<SucursalEspecialidad?> ObtenerConfiguracionActivaAsync(int idSucursal, int idEspecialidad);
        Task<SucursalEspecialidad?> ObtenerConfiguracionAsync(int idSucursal, int idEspecialidad);
        Task<IReadOnlyCollection<SucursalEspecialidad>> ObtenerActivasPorSucursalAsync(int idSucursal);
        Task<SucursalEspecialidad> CrearAsync(SucursalEspecialidad sucursalEspecialidad);
        Task<SucursalEspecialidad> ActualizarAsync(SucursalEspecialidad sucursalEspecialidad);
    }
}
