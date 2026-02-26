using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;
using Clynic.Infrastructure.Data;
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

        public async Task<Cita> CrearAsync(Cita cita)
        {
            if (cita == null)
            {
                throw new ArgumentNullException(nameof(cita));
            }

            cita.FechaCreacion = DateTime.UtcNow;

            await _context.Citas.AddAsync(cita);
            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(cita.Id)
                ?? throw new InvalidOperationException("No se pudo recuperar la cita creada.");
        }

        public async Task<Cita?> ObtenerPorIdAsync(int id)
        {
            return await _context.Citas
                .Include(c => c.Paciente)
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
    }
}
