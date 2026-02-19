using Clynic.Application.DTOs.Clinicas;

namespace Clynic.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz del servicio de Clínicas con lógica de negocio
    /// </summary>
    public interface IClinicaService
    {
        /// <summary>
        /// Obtiene todas las clínicas activas
        /// </summary>
        Task<IEnumerable<ClinicaResponseDto>> ObtenerTodasAsync();

        /// <summary>
        /// Obtiene una clínica por su ID
        /// </summary>
        Task<ClinicaResponseDto?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Crea una nueva clínica
        /// </summary>
        Task<ClinicaResponseDto> CrearAsync(CreateClinicaDto createDto);
    }
}
