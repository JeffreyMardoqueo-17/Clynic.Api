using Clynic.Application.DTOs.LandingPages;

namespace Clynic.Application.Interfaces.Services
{
    public interface ILandingPageConfigService
    {
        Task<LandingPageConfigResponseDto> ObtenerPorClinicaAsync(int idClinica);
        Task<LandingPageConfigResponseDto> GuardarAsync(UpsertLandingPageConfigDto dto);
        Task<LandingPublicResponseDto?> ObtenerPublicaAsync(string clinicaSlug);
    }
}
