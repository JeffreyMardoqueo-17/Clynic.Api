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

        /// <summary>
        /// Actualiza un horario existente
        /// </summary>
        Task<HorarioSucursalResponseDto> ActualizarAsync(int id, UpdateHorarioSucursalDto updateDto);

        /// <summary>
        /// Elimina un horario existente
        /// </summary>
        Task<bool> EliminarAsync(int id);

        /// <summary>
        /// Obtiene los asuetos de una sucursal
        /// </summary>
        Task<IEnumerable<AsuetoSucursalResponseDto>> ObtenerAsuetosPorSucursalAsync(int idSucursal);

        /// <summary>
        /// Crea un asueto para una sucursal
        /// </summary>
        Task<AsuetoSucursalResponseDto> CrearAsuetoAsync(CreateAsuetoSucursalDto createDto);

        /// <summary>
        /// Elimina un asueto existente
        /// </summary>
        Task<bool> EliminarAsuetoAsync(int id);
    }
}
