using Clynic.Application.DTOs.Citas;
using Clynic.Domain.Models.Enums;

namespace Clynic.Application.Interfaces.Services
{
    public interface ICitaService
    {
        Task<CatalogoCitaPublicaDto> ObtenerCatalogoPublicoAsync(int idClinica);
        Task<HorariosDisponiblesCitaDto> ObtenerHorariosDisponiblesPublicoAsync(
            int idClinica,
            int idSucursal,
            DateTime fecha,
            int idEspecialidad,
            IEnumerable<int> idsServicios,
            int intervaloMin = 30);
        Task<CitaResponseDto> CrearPublicaAsync(CreateCitaPublicaDto dto);
        Task<CitaResponseDto> CrearInternaAsync(CreateCitaInternaDto dto);
        Task<CitaResponseDto?> ObtenerPorIdAsync(int idCita);
        Task<IEnumerable<CitaResponseDto>> ObtenerPorClinicaAsync(
            int idClinica,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            EstadoCita? estado = null);
        Task<CitaResponseDto?> AsignarDoctorAsync(int idCita, AsignarDoctorCitaDto dto);
        Task<CitaResponseDto?> CambiarEstadoAsync(int idCita, CambiarEstadoCitaDto dto, string rolEjecutor, int idUsuarioEjecutor);
        Task<ConsultaMedicaResponseDto> RegistrarConsultaAsync(int idCita, int idDoctorEjecutor, RegistrarConsultaMedicaDto dto);
    }
}
