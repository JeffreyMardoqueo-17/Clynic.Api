using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class CitaActividadRepository : ICitaActividadRepository
    {
        private readonly ClynicDbContext _context;

        public CitaActividadRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CitaActividad> CrearAsync(CitaActividad actividad)
        {
            if (actividad == null)
            {
                throw new ArgumentNullException(nameof(actividad));
            }

            actividad.FechaCreacion = DateTime.UtcNow;
            await _context.Set<CitaActividad>().AddAsync(actividad);
            await _context.SaveChangesAsync();
            return actividad;
        }

        public async Task<IReadOnlyCollection<CitaActividad>> ObtenerPorClinicaAsync(
            int idClinica,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int maxResultados = 100)
        {
            var query = _context.Set<CitaActividad>()
                .Where(a => a.IdClinica == idClinica);

            if (fechaDesde.HasValue)
            {
                query = query.Where(a => a.FechaCreacion >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                query = query.Where(a => a.FechaCreacion <= fechaHasta.Value);
            }

            var limite = maxResultados <= 0 ? 100 : Math.Min(maxResultados, 300);
            return await query
                .OrderByDescending(a => a.FechaCreacion)
                .Take(limite)
                .ToListAsync();
        }
    }
}
