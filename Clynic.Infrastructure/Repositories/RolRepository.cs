using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly ClynicDbContext _context;

        public RolRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Rol?> ObtenerPorIdAsync(int id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Rol?> ObtenerPorNombreAsync(int idClinica, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            var valor = nombre.Trim().ToLower();
            return await _context.Roles.FirstOrDefaultAsync(r =>
                r.Nombre.ToLower() == valor &&
                (!r.IdClinica.HasValue || r.IdClinica == idClinica));
        }

        public async Task<Rol?> ObtenerPorNombreEnSucursalAsync(int idClinica, int idSucursal, string nombre)
        {
            if (idSucursal <= 0 || string.IsNullOrWhiteSpace(nombre))
            {
                return null;
            }

            var valor = nombre.Trim().ToLower();
            return await _context.Roles.FirstOrDefaultAsync(r =>
                (!r.IdClinica.HasValue || r.IdClinica == idClinica) &&
                r.IdSucursal == idSucursal &&
                r.Nombre.ToLower() == valor);
        }

        public async Task<IReadOnlyCollection<Rol>> ObtenerActivosAsync(int idClinica)
        {
            return await _context.Roles
                .Where(r => r.Activo && (!r.IdClinica.HasValue || r.IdClinica == idClinica))
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Rol>> ObtenerActivosPorSucursalAsync(int idClinica, int idSucursal)
        {
            return await _context.Roles
                .Where(r => r.Activo && r.IdSucursal == idSucursal && (!r.IdClinica.HasValue || r.IdClinica == idClinica))
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }

        public async Task<Rol> CrearAsync(Rol rol)
        {
            if (rol == null)
            {
                throw new ArgumentNullException(nameof(rol));
            }

            await _context.Roles.AddAsync(rol);
            await _context.SaveChangesAsync();
            return rol;
        }
    }
}
