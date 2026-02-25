using Microsoft.EntityFrameworkCore;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;

namespace Clynic.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ClynicDbContext _context;

        public UsuarioRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> ObtenerPorClinicaAsync(int idClinica)
        {
            return await _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .Where(u => u.IdClinica == idClinica)
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> ObtenerPorClinicaYSucursalAsync(int idClinica, int idSucursal)
        {
            return await _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .Where(u => u.IdClinica == idClinica && u.IdSucursal == idSucursal)
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return null;

            return await _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower());
        }

        public async Task<Usuario> CrearAsync(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            usuario.FechaCreacion = DateTime.UtcNow;
            usuario.Activo = true;

            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<Usuario> ActualizarAsync(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            usuario.Activo = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExisteCorreoAsync(string correo, int? idExcluir = null)
        {
            var query = _context.Usuarios
                .Where(u => u.Correo.ToLower() == correo.ToLower());

            if (idExcluir.HasValue)
            {
                query = query.Where(u => u.Id != idExcluir.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Id == id);
        }
    }
}
