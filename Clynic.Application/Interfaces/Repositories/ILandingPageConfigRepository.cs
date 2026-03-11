using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface ILandingPageConfigRepository
    {
        Task<LandingPageConfig?> ObtenerPorClinicaAsync(int idClinica);
        Task<LandingPageConfig?> ObtenerPublicadaPorDominioAsync(string dominioBase);
        Task<LandingPageConfig?> ObtenerCualquierPublicadaAsync();
        Task<LandingPageConfig> CrearAsync(LandingPageConfig config);
        Task<LandingPageConfig> ActualizarAsync(LandingPageConfig config);
    }
}
