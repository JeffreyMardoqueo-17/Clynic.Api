using Clynic.Application.DTOs.Servicios;

namespace Clynic.Application.Interfaces.Services
{
    public interface IServicioService
    {
        Task<IEnumerable<ServicioResponseDto>> ObtenerPorClinicaAsync(int idClinica, string? nombre = null, bool incluirInactivos = false);
        Task<IReadOnlyCollection<CapacidadEspecialidadDiaDto>> ObtenerCapacidadPorEspecialidadAsync(
            int idClinica,
            DateTime fechaDesde,
            DateTime fechaHasta,
            int? idSucursal = null,
            int horasLaborablesDia = 8,
            int minutosAlmuerzoDia = 60);
        Task<ServicioResponseDto?> ObtenerPorIdAsync(int id, bool incluirInactivos = false);
        Task<ServicioResponseDto> CrearAsync(CreateServicioDto createDto);
        Task<ServicioResponseDto?> ActualizarAsync(int id, UpdateServicioDto updateDto);
        Task<bool> EliminarAsync(int id);
    }
}
