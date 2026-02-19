using Microsoft.EntityFrameworkCore;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;

namespace Clynic.Infrastructure.Repositories
{
    /// <summary>
    /// Implementacion del repositorio de Sucursales usando Entity Framework Core
    /// </summary>
    public class SucursalRepository : ISucursalRepository
    {
        private readonly ClynicDbContext _context;

        public SucursalRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Sucursal>> ObtenerTodasAsync()
        {
            return await _context.Sucursales
                .Where(s => s.Activa)
                .OrderBy(s => s.Nombre)
                .ToListAsync();
        }

        public async Task<Sucursal?> ObtenerPorIdAsync(int id)
        {
            return await _context.Sucursales
                .FirstOrDefaultAsync(s => s.Id == id && s.Activa);
        }

        public async Task<Sucursal> CrearAsync(Sucursal sucursal)
        {
            if (sucursal == null)
                throw new ArgumentNullException(nameof(sucursal));

            sucursal.Activa = true;

            await _context.Sucursales.AddAsync(sucursal);
            await _context.SaveChangesAsync();

            return sucursal;
        }

        public async Task<Sucursal> ActualizarAsync(Sucursal sucursal)
        {
            if (sucursal == null)
                throw new ArgumentNullException(nameof(sucursal));

            _context.Sucursales.Update(sucursal);
            await _context.SaveChangesAsync();

            return sucursal;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var sucursal = await _context.Sucursales.FindAsync(id);
            if (sucursal == null)
                return false;

            sucursal.Activa = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int idClinica, int? idExcluir = null)
        {
            var query = _context.Sucursales
                .Where(s => s.Activa && s.IdClinica == idClinica && s.Nombre.ToLower() == nombre.ToLower());

            if (idExcluir.HasValue)
            {
                query = query.Where(s => s.Id != idExcluir.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Sucursales
                .AnyAsync(s => s.Id == id && s.Activa);
        }
    }
}
