using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class CitaServicioRepository : ICitaServicioRepository
    {
        private readonly ClynicDbContext _context;

        public CitaServicioRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyCollection<CitaServicio>> ObtenerPorCitaAsync(int idCita)
        {
            return await _context.CitasServicio
                .Include(cs => cs.Servicio)
                .Include(cs => cs.Cita)
                .Where(cs => cs.IdCita == idCita)
                .OrderBy(cs => cs.Id)
                .ToListAsync();
        }

        public async Task<CitaServicio?> ObtenerPorIdAsync(int idCitaServicio)
        {
            return await _context.CitasServicio
                .Include(cs => cs.Servicio)
                .Include(cs => cs.Cita)
                .FirstOrDefaultAsync(cs => cs.Id == idCitaServicio);
        }

        public async Task<CitaServicio> CrearAsync(CitaServicio citaServicio)
        {
            if (citaServicio == null)
            {
                throw new ArgumentNullException(nameof(citaServicio));
            }

            await _context.CitasServicio.AddAsync(citaServicio);
            await _context.SaveChangesAsync();

            return await ObtenerPorIdAsync(citaServicio.Id)
                ?? throw new InvalidOperationException("No se pudo recuperar el detalle de cita creado.");
        }

        public async Task<bool> EliminarAsync(int idCitaServicio)
        {
            var existente = await _context.CitasServicio
                .FirstOrDefaultAsync(cs => cs.Id == idCitaServicio);

            if (existente == null)
            {
                return false;
            }

            _context.CitasServicio.Remove(existente);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
