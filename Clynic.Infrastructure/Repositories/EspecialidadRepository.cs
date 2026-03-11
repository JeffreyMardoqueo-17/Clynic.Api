using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class EspecialidadRepository : IEspecialidadRepository
    {
        private readonly ClynicDbContext _context;

        public EspecialidadRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Especialidad?> ObtenerPorIdAsync(int id)
        {
            return await _context.Especialidades.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Especialidad?> ObtenerPorNombreAsync(int idClinica, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            var valor = nombre.Trim().ToLower();
            return await _context.Especialidades
                .FirstOrDefaultAsync(e =>
                    e.Nombre.ToLower() == valor &&
                    (!e.IdClinica.HasValue || e.IdClinica == idClinica));
        }

        public async Task<IReadOnlyCollection<Especialidad>> ObtenerActivasAsync(int idClinica)
        {
            return await _context.Especialidades
                .Where(e => e.IdClinica == idClinica && e.Activa)
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteActivaAsync(int idClinica, int id)
        {
            return await _context.Especialidades.AnyAsync(e => e.IdClinica == idClinica && e.Id == id && e.Activa);
        }

        public async Task<Especialidad> CrearAsync(Especialidad especialidad)
        {
            if (especialidad == null)
            {
                throw new ArgumentNullException(nameof(especialidad));
            }

            await _context.Especialidades.AddAsync(especialidad);
            await _context.SaveChangesAsync();
            return especialidad;
        }
    }
}
