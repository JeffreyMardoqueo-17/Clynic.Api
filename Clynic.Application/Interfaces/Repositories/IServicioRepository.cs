using Clynic.Domain.Models;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IServicioRepository
    {
        Task<IEnumerable<Servicio>> ObtenerPorClinicaAsync(int idClinica, string? nombre = null, bool incluirInactivos = false);
        Task<Servicio?> ObtenerPorIdAsync(int id, bool incluirInactivos = false);
        Task<Servicio> CrearAsync(Servicio servicio);
        Task<Servicio> ActualizarAsync(Servicio servicio);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteNombreAsync(string nombreServicio, int idClinica, int? idExcluir = null);
        Task<bool> ExisteAsync(int id);
    }
}
