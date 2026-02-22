using Clynic.Application.DTOs.HorariosSucursal;

namespace Clynic.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz del servicio de Horarios de Sucursal con logica de negocio
    /// </summary>
    public interface IHorarioSucursalService
    {
        /// <summary>
        /// Obtiene todos los horarios
        /// </summary>
        Task<IEnumerable<HorarioSucursalResponseDto>> ObtenerTodosAsync();

        /// <summary>
        /// Obtiene un horario por su ID
        /// </summary>
        Task<HorarioSucursalResponseDto?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Obtiene todos los horarios de una sucursal
        /// </summary>
        Task<IEnumerable<HorarioSucursalResponseDto>> ObtenerPorSucursalAsync(int idSucursal);

        /// <summary>
        /// Crea un nuevo horario de sucursal
        /// </summary>
        Task<HorarioSucursalResponseDto> CrearAsync(CreateHorarioSucursalDto createDto);
    }
}
