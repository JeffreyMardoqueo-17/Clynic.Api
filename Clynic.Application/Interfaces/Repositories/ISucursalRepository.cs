using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz del repositorio para operaciones de base de datos de Sucursales
    /// </summary>
    public interface ISucursalRepository
    {
        /// <summary>
        /// Obtiene todas las sucursales activas
        /// </summary>
        Task<IEnumerable<Sucursal>> ObtenerTodasAsync();

        /// <summary>
        /// Obtiene una sucursal por su ID
        /// </summary>
        Task<Sucursal?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Crea una nueva sucursal
        /// </summary>
        Task<Sucursal> CrearAsync(Sucursal sucursal);

        /// <summary>
        /// Actualiza una sucursal existente
        /// </summary>
        Task<Sucursal> ActualizarAsync(Sucursal sucursal);

        /// <summary>
        /// Elimina (desactiva) una sucursal
        /// </summary>
        Task<bool> EliminarAsync(int id);

        /// <summary>
        /// Verifica si existe una sucursal con el nombre especificado en la clinica
        /// </summary>
        Task<bool> ExisteNombreAsync(string nombre, int idClinica, int? idExcluir = null);

        /// <summary>
        /// Verifica si existe una sucursal por ID
        /// </summary>
        Task<bool> ExisteAsync(int id);
    }
}
