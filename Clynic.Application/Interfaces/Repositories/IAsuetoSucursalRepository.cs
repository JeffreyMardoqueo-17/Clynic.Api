using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IAsuetoSucursalRepository
    {
        Task<IEnumerable<AsuetoSucursal>> ObtenerPorSucursalAsync(int idSucursal);
        Task<AsuetoSucursal?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteEnFechaAsync(int idSucursal, DateOnly fecha);
        Task<AsuetoSucursal> CrearAsync(AsuetoSucursal asueto);
        Task<bool> EliminarAsync(int id);
    }
}