using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IRolRepository
    {
        Task<Rol?> ObtenerPorIdAsync(int id);
        Task<Rol?> ObtenerPorNombreAsync(int idClinica, string nombre);
        Task<Rol?> ObtenerPorNombreEnSucursalAsync(int idClinica, int idSucursal, string nombre);
        Task<IReadOnlyCollection<Rol>> ObtenerActivosAsync(int idClinica);
        Task<IReadOnlyCollection<Rol>> ObtenerActivosPorSucursalAsync(int idClinica, int idSucursal);
        Task<Rol> CrearAsync(Rol rol);
    }
}
