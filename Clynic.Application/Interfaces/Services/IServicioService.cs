using Clynic.Application.DTOs.Servicios;

namespace Clynic.Application.Interfaces.Services
{
    public interface IServicioService
    {
        Task<IEnumerable<ServicioResponseDto>> ObtenerPorClinicaAsync(int idClinica, string? nombre = null, bool incluirInactivos = false);
        Task<ServicioResponseDto?> ObtenerPorIdAsync(int id, bool incluirInactivos = false);
        Task<ServicioResponseDto> CrearAsync(CreateServicioDto createDto);
        Task<ServicioResponseDto?> ActualizarAsync(int id, UpdateServicioDto updateDto);
        Task<bool> EliminarAsync(int id);
    }
}
