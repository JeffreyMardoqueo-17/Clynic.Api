using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class SucursalEspecialidadRepository : ISucursalEspecialidadRepository
    {
        private readonly ClynicDbContext _context;

        public SucursalEspecialidadRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<SucursalEspecialidad?> ObtenerConfiguracionActivaAsync(int idSucursal, int idEspecialidad)
        {
            return await _context.SucursalesEspecialidad
                .Include(se => se.Especialidad)
                .FirstOrDefaultAsync(se =>
                    se.IdSucursal == idSucursal
                    && se.IdEspecialidad == idEspecialidad
                    && se.Activa
                    && se.Especialidad != null
                    && se.Especialidad.Activa);
        }

        public async Task<SucursalEspecialidad?> ObtenerConfiguracionAsync(int idSucursal, int idEspecialidad)
        {
            return await _context.SucursalesEspecialidad
                .Include(se => se.Especialidad)
                .FirstOrDefaultAsync(se =>
                    se.IdSucursal == idSucursal
                    && se.IdEspecialidad == idEspecialidad
                    && se.Especialidad != null);
        }

        public async Task<IReadOnlyCollection<SucursalEspecialidad>> ObtenerActivasPorSucursalAsync(int idSucursal)
        {
            return await _context.SucursalesEspecialidad
                .Include(se => se.Especialidad)
                .Where(se =>
                    se.IdSucursal == idSucursal
                    && se.Activa
                    && se.Especialidad != null
                    && se.Especialidad.Activa)
                .OrderBy(se => se.Especialidad!.Nombre)
                .ToListAsync();
        }

        public async Task<SucursalEspecialidad> CrearAsync(SucursalEspecialidad sucursalEspecialidad)
        {
            if (sucursalEspecialidad == null)
            {
                throw new ArgumentNullException(nameof(sucursalEspecialidad));
            }

            await _context.SucursalesEspecialidad.AddAsync(sucursalEspecialidad);
            await _context.SaveChangesAsync();

            return await _context.SucursalesEspecialidad
                .Include(se => se.Especialidad)
                .FirstAsync(se => se.Id == sucursalEspecialidad.Id);
        }

        public async Task<SucursalEspecialidad> ActualizarAsync(SucursalEspecialidad sucursalEspecialidad)
        {
            if (sucursalEspecialidad == null)
            {
                throw new ArgumentNullException(nameof(sucursalEspecialidad));
            }

            _context.SucursalesEspecialidad.Update(sucursalEspecialidad);
            await _context.SaveChangesAsync();

            return await _context.SucursalesEspecialidad
                .Include(se => se.Especialidad)
                .FirstAsync(se => se.Id == sucursalEspecialidad.Id);
        }
    }
}
