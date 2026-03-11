using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class RolEspecialidadRepository : IRolEspecialidadRepository
    {
        private readonly ClynicDbContext _context;

        public RolEspecialidadRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RolEspecialidad?> ObtenerActivaAsync(int idRol, int idEspecialidad)
        {
            return await _context.RolesEspecialidad
                .Include(x => x.Especialidad)
                .FirstOrDefaultAsync(x => x.IdRol == idRol && x.IdEspecialidad == idEspecialidad && x.Activa);
        }

        public async Task<IReadOnlyCollection<RolEspecialidad>> ObtenerActivasPorRolAsync(int idRol)
        {
            return await _context.RolesEspecialidad
                .Include(x => x.Especialidad)
                .Where(x => x.IdRol == idRol && x.Activa)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<RolEspecialidad> CrearAsync(RolEspecialidad rolEspecialidad)
        {
            if (rolEspecialidad == null)
            {
                throw new ArgumentNullException(nameof(rolEspecialidad));
            }

            await _context.RolesEspecialidad.AddAsync(rolEspecialidad);
            await _context.SaveChangesAsync();
            return rolEspecialidad;
        }
    }
}
