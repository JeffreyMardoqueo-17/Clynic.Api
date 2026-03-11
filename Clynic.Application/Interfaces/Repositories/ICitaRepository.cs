using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface ICitaRepository
    {
        Task<int> ContarTraslapesHorarioAsync(
            int idClinica,
            int idSucursal,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null);

        Task<bool> ExisteTraslapeHorarioDoctorAsync(
            int idClinica,
            int idSucursal,
            int idDoctor,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null);

        Task<bool> ExisteTraslapeHorarioAsync(
            int idClinica,
            int idSucursal,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null);

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
        Task<int> ContarPorClinicaYFechaAsync(
            int idClinica,
            DateTime fechaDesdeInclusive,
            DateTime fechaHastaExclusive,
            int? idSucursal = null,
            EstadoCita? estado = null);
        Task<int> ContarCitasActivasEspecialidadDiaAsync(
            int idClinica,
            int idSucursal,
            int idEspecialidad,
            DateTime fechaDia,
            int? idCitaExcluir = null);
        Task<int> ContarTraslapesEspecialidadHorarioAsync(
            int idClinica,
            int idSucursal,
            int idEspecialidad,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null);
        Task<bool> ExisteTraslapePacienteAsync(
            int idClinica,
            int idPaciente,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null);
        Task<IReadOnlyList<(DateTime Fecha, int Total)>> ObtenerTotalesPorDiaAsync(
            int idClinica,
            DateTime fechaDesdeInclusive,
            DateTime fechaHastaExclusive,
            int? idSucursal = null);
    }
}
