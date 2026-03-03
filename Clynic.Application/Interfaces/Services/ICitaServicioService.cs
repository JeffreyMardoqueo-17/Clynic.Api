using Clynic.Application.DTOs.CitaServicios;

namespace Clynic.Application.Interfaces.Services
{
    public interface ICitaServicioService
    {
        Task<IReadOnlyCollection<CitaServicioResponseDto>> ObtenerPorCitaAsync(int idCita);
        Task<CitaServicioResponseDto?> ObtenerPorIdAsync(int idCitaServicio);
        Task<CitaServicioResponseDto> CrearAsync(CreateCitaServicioDto dto);
        Task<bool> EliminarAsync(int idCitaServicio);
    }
}
