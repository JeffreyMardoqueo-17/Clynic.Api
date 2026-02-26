using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly ClynicDbContext _context;

        public ServicioRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Servicio>> ObtenerPorClinicaAsync(int idClinica, string? nombre = null, bool incluirInactivos = false)
        {
            var query = _context.Servicios
                .Where(s => s.IdClinica == idClinica);

            if (!incluirInactivos)
            {
                query = query.Where(s => s.Activo);
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var termino = nombre.Trim().ToLower();
                query = query.Where(s => s.NombreServicio.ToLower().Contains(termino));
            }

            return await query
                .OrderBy(s => s.NombreServicio)
                .ToListAsync();
        }

        public async Task<Servicio?> ObtenerPorIdAsync(int id, bool incluirInactivos = false)
        {
            var query = _context.Servicios.Where(s => s.Id == id);

            if (!incluirInactivos)
            {
                query = query.Where(s => s.Activo);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Servicio> CrearAsync(Servicio servicio)
        {
            if (servicio == null)
                throw new ArgumentNullException(nameof(servicio));

            servicio.Activo = true;

            await _context.Servicios.AddAsync(servicio);
            await _context.SaveChangesAsync();

            return servicio;
        }

        public async Task<Servicio> ActualizarAsync(Servicio servicio)
        {
            if (servicio == null)
                throw new ArgumentNullException(nameof(servicio));

            _context.Servicios.Update(servicio);
            await _context.SaveChangesAsync();

            return servicio;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var servicio = await _context.Servicios.FirstOrDefaultAsync(s => s.Id == id);
            if (servicio == null)
                return false;

            servicio.Activo = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombreServicio, int idClinica, int? idExcluir = null)
        {
            var query = _context.Servicios
                .Where(s => s.IdClinica == idClinica && s.NombreServicio.ToLower() == nombreServicio.ToLower());

            if (idExcluir.HasValue)
            {
                query = query.Where(s => s.Id != idExcluir.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Servicios.AnyAsync(s => s.Id == id && s.Activo);
        }
    }
}
