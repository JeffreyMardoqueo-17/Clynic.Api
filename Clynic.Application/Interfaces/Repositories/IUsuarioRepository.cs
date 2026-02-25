using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<IEnumerable<Usuario>> ObtenerPorClinicaAsync(int idClinica);
        Task<IEnumerable<Usuario>> ObtenerPorClinicaYSucursalAsync(int idClinica, int idSucursal);
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task<Usuario> CrearAsync(Usuario usuario);
        Task<Usuario> ActualizarAsync(Usuario usuario);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteCorreoAsync(string correo, int? idExcluir = null);
        Task<bool> ExisteAsync(int id);
    }
}
