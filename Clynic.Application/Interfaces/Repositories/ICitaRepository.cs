using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface ICitaRepository
    {
        Task<Cita> CrearAsync(Cita cita);
        Task<Cita?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Cita>> ObtenerPorClinicaAsync(
            int idClinica,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            EstadoCita? estado = null);
        Task<Cita> ActualizarAsync(Cita cita);
        Task<ConsultaMedica?> ObtenerConsultaPorCitaAsync(int idCita);
        Task<ConsultaMedica> CrearConsultaAsync(ConsultaMedica consulta);
    }
}
