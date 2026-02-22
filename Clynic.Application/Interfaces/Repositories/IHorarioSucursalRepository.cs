using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz del repositorio para operaciones de base de datos de Horarios de Sucursal
    /// </summary>
    public interface IHorarioSucursalRepository
    {
        /// <summary>
        /// Obtiene todos los horarios
        /// </summary>
        Task<IEnumerable<HorarioSucursal>> ObtenerTodosAsync();

        /// <summary>
        /// Obtiene todos los horarios de una sucursal
        /// </summary>
        Task<IEnumerable<HorarioSucursal>> ObtenerPorSucursalAsync(int idSucursal);

        /// <summary>
        /// Obtiene un horario por su ID
        /// </summary>
        Task<HorarioSucursal?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Crea un nuevo horario
        /// </summary>
        Task<HorarioSucursal> CrearAsync(HorarioSucursal horario);

        /// <summary>
        /// Verifica si existe cruce de horario en la misma sucursal y dia
        /// </summary>
        Task<bool> ExisteCruceHorarioAsync(int idSucursal, int diaSemana, TimeSpan horaInicio, TimeSpan horaFin);
    }
}
