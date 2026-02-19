using Clynic.Application.DTOs.Sucursales;

namespace Clynic.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz del servicio de Sucursales con logica de negocio
    /// </summary>
    public interface ISucursalService
    {
        /// <summary>
        /// Obtiene todas las sucursales activas
        /// </summary>
        Task<IEnumerable<SucursalResponseDto>> ObtenerTodasAsync();

        /// <summary>
        /// Obtiene una sucursal por su ID
        /// </summary>
        Task<SucursalResponseDto?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Crea una nueva sucursal
        /// </summary>
        Task<SucursalResponseDto> CrearAsync(CreateSucursalDto createDto);
    }
}
