using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz del repositorio para operaciones de base de datos de Clínicas
    /// </summary>
    public interface IClinicaRepository
    {
        /// <summary>
        /// Obtiene todas las clínicas activas
        /// </summary>
        Task<IEnumerable<Clinica>> ObtenerTodasAsync();

        /// <summary>
        /// Obtiene una clínica por su ID
        /// </summary>
        Task<Clinica?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Crea una nueva clínica
        /// </summary>
        Task<Clinica> CrearAsync(Clinica clinica);

        /// <summary>
        /// Actualiza una clínica existente
        /// </summary>
        Task<Clinica> ActualizarAsync(Clinica clinica);

        /// <summary>
        /// Elimina (desactiva) una clínica
        /// </summary>
        Task<bool> EliminarAsync(int id);

        /// <summary>
        /// Verifica si existe una clínica con el nombre especificado
        /// </summary>
        Task<bool> ExisteNombreAsync(string nombre, int? idExcluir = null);

        /// <summary>
        /// Verifica si existe una clínica por ID
        /// </summary>
        Task<bool> ExisteAsync(int id);
    }
}
