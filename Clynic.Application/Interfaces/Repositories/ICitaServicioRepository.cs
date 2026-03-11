using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface ICitaServicioRepository
    {
        Task<IReadOnlyCollection<CitaServicio>> ObtenerPorCitaAsync(int idCita);
        Task<CitaServicio?> ObtenerPorIdAsync(int idCitaServicio);
        Task<CitaServicio> CrearAsync(CitaServicio citaServicio);
        Task<bool> EliminarAsync(int idCitaServicio);
    }
}
