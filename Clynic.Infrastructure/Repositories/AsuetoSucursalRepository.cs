using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class AsuetoSucursalRepository : IAsuetoSucursalRepository
    {
        private readonly ClynicDbContext _context;

        public AsuetoSucursalRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<AsuetoSucursal>> ObtenerPorSucursalAsync(int idSucursal)
        {
            return await _context.AsuetosSucursal
                .Where(a => a.IdSucursal == idSucursal)
                .OrderBy(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<AsuetoSucursal?> ObtenerPorIdAsync(int id)
        {
            return await _context.AsuetosSucursal
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> ExisteEnFechaAsync(int idSucursal, DateOnly fecha)
        {
            return await _context.AsuetosSucursal
                .AnyAsync(a => a.IdSucursal == idSucursal && a.Fecha == fecha);
        }

        public async Task<AsuetoSucursal> CrearAsync(AsuetoSucursal asueto)
        {
            if (asueto == null)
                throw new ArgumentNullException(nameof(asueto));

            await _context.AsuetosSucursal.AddAsync(asueto);
            await _context.SaveChangesAsync();

            return asueto;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var asueto = await _context.AsuetosSucursal.FirstOrDefaultAsync(a => a.Id == id);
            if (asueto == null)
            {
                return false;
            }

            _context.AsuetosSucursal.Remove(asueto);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}