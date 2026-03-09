using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;
using Clynic.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class CitaRepository : ICitaRepository
    {
        private readonly ClynicDbContext _context;

        public CitaRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> ExisteTraslapeHorarioAsync(
            int idClinica,
            int idSucursal,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null)
        {
            var query = QueryCitasActivasConTraslape(idClinica, idSucursal, fechaHoraInicio, fechaHoraFin);

            if (idCitaExcluir.HasValue)
            {
                query = query.Where(c => c.Id != idCitaExcluir.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> ContarTraslapesHorarioAsync(
            int idClinica,
            int idSucursal,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null)
        {
            var query = QueryCitasActivasConTraslape(idClinica, idSucursal, fechaHoraInicio, fechaHoraFin);

            if (idCitaExcluir.HasValue)
            {
                query = query.Where(c => c.Id != idCitaExcluir.Value);
            }

            return await query.CountAsync();
        }

        public async Task<bool> ExisteTraslapeHorarioDoctorAsync(
            int idClinica,
            int idSucursal,
            int idDoctor,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null)
        {
            var query = QueryCitasActivasConTraslape(idClinica, idSucursal, fechaHoraInicio, fechaHoraFin)
                .Where(c => c.IdDoctor == idDoctor);

            if (idCitaExcluir.HasValue)
            {
                query = query.Where(c => c.Id != idCitaExcluir.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Cita> CrearAsync(Cita cita)
        {
            if (cita == null)
            {
                throw new ArgumentNullException(nameof(cita));
            }

            cita.FechaCreacion = DateTime.UtcNow;

            try
            {
                await _context.Citas.AddAsync(cita);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx) when (EsChoqueHorarioPorIndiceUnico(dbEx))
            {
                throw new InvalidOperationException("Ya existe una cita reservada para esa hora en esta sucursal. Selecciona otro horario.");
            }

            return await ObtenerPorIdAsync(cita.Id)
                ?? throw new InvalidOperationException("No se pudo recuperar la cita creada.");
        }

        public async Task<Cita?> ObtenerPorIdAsync(int id)
        {
            return await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Especialidad)
                .Include(c => c.CitaServicios)
                    .ThenInclude(cs => cs.Servicio)
                .Include(c => c.ConsultaMedica)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Cita>> ObtenerPorClinicaAsync(
            int idClinica,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            EstadoCita? estado = null)
        {
            var query = _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Especialidad)
                .Include(c => c.CitaServicios)
                    .ThenInclude(cs => cs.Servicio)
                .Include(c => c.ConsultaMedica)
                .Where(c => c.IdClinica == idClinica);

            if (idSucursal.HasValue)
            {
                query = query.Where(c => c.IdSucursal == idSucursal.Value);
            }

            if (estado.HasValue)
            {
                query = query.Where(c => c.Estado == estado.Value);
            }

            if (fechaDesde.HasValue)
            {
                query = query.Where(c => c.FechaHoraInicioPlan >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(c => c.FechaHoraInicioPlan <= fechaHasta.Value);
            }

            return await query
                .OrderByDescending(c => c.FechaHoraInicioPlan)
                .ToListAsync();
        }

        public async Task<Cita> ActualizarAsync(Cita cita)
        {
            if (cita == null)
            {
                throw new ArgumentNullException(nameof(cita));
            }

            _context.Citas.Update(cita);
            await _context.SaveChangesAsync();
            return cita;
        }

        public async Task<ConsultaMedica?> ObtenerConsultaPorCitaAsync(int idCita)
        {
            return await _context.ConsultasMedicas
                .FirstOrDefaultAsync(c => c.IdCita == idCita);
        }

        public async Task<ConsultaMedica> CrearConsultaAsync(ConsultaMedica consulta)
        {
            if (consulta == null)
            {
                throw new ArgumentNullException(nameof(consulta));
            }

            consulta.FechaCreacion = DateTime.UtcNow;
            await _context.ConsultasMedicas.AddAsync(consulta);
            await _context.SaveChangesAsync();

            return consulta;
        }

        public async Task<int> ContarPorClinicaYFechaAsync(
            int idClinica,
            DateTime fechaDesdeInclusive,
            DateTime fechaHastaExclusive,
            int? idSucursal = null,
            EstadoCita? estado = null)
        {
            var query = _context.Citas.Where(c =>
                c.IdClinica == idClinica &&
                c.FechaHoraInicioPlan >= fechaDesdeInclusive &&
                c.FechaHoraInicioPlan < fechaHastaExclusive);

            if (idSucursal.HasValue)
            {
                query = query.Where(c => c.IdSucursal == idSucursal.Value);
            }

            if (estado.HasValue)
            {
                query = query.Where(c => c.Estado == estado.Value);
            }

            return await query.CountAsync();
        }

        public async Task<int> ContarCitasActivasEspecialidadDiaAsync(
            int idClinica,
            int idSucursal,
            int idEspecialidad,
            DateTime fechaDia,
            int? idCitaExcluir = null)
        {
            var inicioDia = fechaDia.Date;
            var finDia = inicioDia.AddDays(1);

            var query = _context.Citas.Where(c =>
                c.IdClinica == idClinica
                && c.IdSucursal == idSucursal
                && c.IdEspecialidad == idEspecialidad
                && c.FechaHoraInicioPlan >= inicioDia
                && c.FechaHoraInicioPlan < finDia
                && c.Estado != EstadoCita.Cancelada);

            if (idCitaExcluir.HasValue)
            {
                query = query.Where(c => c.Id != idCitaExcluir.Value);
            }

            return await query.CountAsync();
        }

        public async Task<int> ContarTraslapesEspecialidadHorarioAsync(
            int idClinica,
            int idSucursal,
            int idEspecialidad,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin,
            int? idCitaExcluir = null)
        {
            var query = QueryCitasActivasConTraslape(idClinica, idSucursal, fechaHoraInicio, fechaHoraFin)
                .Where(c => c.IdEspecialidad == idEspecialidad);

            if (idCitaExcluir.HasValue)
            {
                query = query.Where(c => c.Id != idCitaExcluir.Value);
            }

            return await query.CountAsync();
        }

        public async Task<IReadOnlyList<(DateTime Fecha, int Total)>> ObtenerTotalesPorDiaAsync(
            int idClinica,
            DateTime fechaDesdeInclusive,
            DateTime fechaHastaExclusive,
            int? idSucursal = null)
        {
            var query = _context.Citas.Where(c =>
                c.IdClinica == idClinica &&
                c.FechaHoraInicioPlan >= fechaDesdeInclusive &&
                c.FechaHoraInicioPlan < fechaHastaExclusive);

            if (idSucursal.HasValue)
            {
                query = query.Where(c => c.IdSucursal == idSucursal.Value);
            }

            var agregados = await query
                .GroupBy(c => c.FechaHoraInicioPlan.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Total = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            return agregados
                .Select(x => (x.Fecha, x.Total))
                .ToList();
        }

        private static bool EsChoqueHorarioPorIndiceUnico(DbUpdateException ex)
        {
            if (ex.InnerException is not SqlException sqlEx)
            {
                return false;
            }

            var isUniqueViolation = sqlEx.Number is 2601 or 2627;
            if (!isUniqueViolation)
            {
                return false;
            }

            return sqlEx.Message.Contains("UX_Cita_Clinica_Sucursal_FechaHoraInicio_Activa", StringComparison.OrdinalIgnoreCase);
        }

        private IQueryable<Cita> QueryCitasActivasConTraslape(
            int idClinica,
            int idSucursal,
            DateTime fechaHoraInicio,
            DateTime fechaHoraFin)
        {
            return _context.Citas.Where(c =>
                c.IdClinica == idClinica
                && c.IdSucursal == idSucursal
                && c.Estado != EstadoCita.Cancelada
                && c.Estado != EstadoCita.Completada
                && c.FechaHoraInicioPlan < fechaHoraFin
                && c.FechaHoraFinPlan > fechaHoraInicio);
        }
    }
}
