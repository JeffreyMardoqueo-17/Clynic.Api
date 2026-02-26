using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<IEnumerable<Usuario>> ObtenerPorClinicaAsync(int idClinica, string? busquedaNombre = null);
        Task<IEnumerable<Usuario>> ObtenerPorClinicaYSucursalAsync(int idClinica, int idSucursal, string? busquedaNombre = null);
        Task<IEnumerable<Usuario>> ObtenerInactivosPorClinicaAsync(int idClinica, int? idSucursal = null, string? busquedaNombre = null);
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task<Usuario> CrearAsync(Usuario usuario);
        Task<Usuario> ActualizarAsync(Usuario usuario);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteCorreoAsync(string correo, int? idExcluir = null);
        Task<bool> ExisteAsync(int id);
    }
}
