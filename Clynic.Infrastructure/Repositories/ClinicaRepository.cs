using Microsoft.EntityFrameworkCore;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;

namespace Clynic.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de Clínicas usando Entity Framework Core
    /// </summary>
    public class ClinicaRepository : IClinicaRepository
    {
        private readonly ClynicDbContext _context;

        public ClinicaRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Clinica>> ObtenerTodasAsync()
        {
            return await _context.Clinicas
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Clinica?> ObtenerPorIdAsync(int id)
        {
            return await _context.Clinicas
                .FirstOrDefaultAsync(c => c.Id == id && c.Activa);
        }

        public async Task<Clinica> CrearAsync(Clinica clinica)
        {
            if (clinica == null)
                throw new ArgumentNullException(nameof(clinica));

            clinica.FechaCreacion = DateTime.UtcNow;
            clinica.Activa = true;

            await _context.Clinicas.AddAsync(clinica);
            await _context.SaveChangesAsync();

            return clinica;
        }

        public async Task<Clinica> ActualizarAsync(Clinica clinica)
        {
            if (clinica == null)
                throw new ArgumentNullException(nameof(clinica));

            _context.Clinicas.Update(clinica);
            await _context.SaveChangesAsync();

            return clinica;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var clinica = await _context.Clinicas.FindAsync(id);
            if (clinica == null)
                return false;

            // Eliminación lógica
            clinica.Activa = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? idExcluir = null)
        {
            var query = _context.Clinicas
                .Where(c => c.Activa && c.Nombre.ToLower() == nombre.ToLower());

            if (idExcluir.HasValue)
            {
                query = query.Where(c => c.Id != idExcluir.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Clinicas
                .AnyAsync(c => c.Id == id && c.Activa);
        }
    }
}
