using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface ICitaActividadRepository
    {
        Task<CitaActividad> CrearAsync(CitaActividad actividad);
        Task<IReadOnlyCollection<CitaActividad>> ObtenerPorClinicaAsync(
            int idClinica,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int maxResultados = 100);
    }
}
