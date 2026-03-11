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
                .Include(u => u.Rol)
                .Include(u => u.Especialidad)
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> ObtenerPorClinicaAsync(int idClinica, string? busquedaNombre = null)
        {
            var query = _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .Include(u => u.Rol)
                .Include(u => u.Especialidad)
                .Where(u => u.IdClinica == idClinica && u.Activo);

            if (!string.IsNullOrWhiteSpace(busquedaNombre))
            {
                var search = busquedaNombre.Trim().ToLower();
                query = query.Where(u => u.NombreCompleto.ToLower().Contains(search));
            }

            return await query
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> ObtenerPorClinicaYSucursalAsync(int idClinica, int idSucursal, string? busquedaNombre = null)
        {
            var query = _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .Include(u => u.Rol)
                .Include(u => u.Especialidad)
                .Where(u => u.IdClinica == idClinica && u.IdSucursal == idSucursal && u.Activo);

            if (!string.IsNullOrWhiteSpace(busquedaNombre))
            {
                var search = busquedaNombre.Trim().ToLower();
                query = query.Where(u => u.NombreCompleto.ToLower().Contains(search));
            }

            return await query
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> ObtenerInactivosPorClinicaAsync(int idClinica, int? idSucursal = null, string? busquedaNombre = null)
        {
            var query = _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .Include(u => u.Rol)
                .Include(u => u.Especialidad)
                .Where(u => u.IdClinica == idClinica && !u.Activo);

            if (idSucursal.HasValue)
            {
                query = query.Where(u => u.IdSucursal == idSucursal.Value);
            }

            if (!string.IsNullOrWhiteSpace(busquedaNombre))
            {
                var search = busquedaNombre.Trim().ToLower();
                query = query.Where(u => u.NombreCompleto.ToLower().Contains(search));
            }

            return await query
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();
        }

            public async Task<int> ContarActivosPorClinicaAsync(int idClinica)
            {
                return await _context.Usuarios
                .CountAsync(u => u.IdClinica == idClinica && u.Activo);
            }

            public async Task<int> ContarActivosPorClinicaYSucursalAsync(int idClinica, int idSucursal)
            {
                return await _context.Usuarios
                    .CountAsync(u =>
                        u.IdClinica == idClinica &&
                        u.IdSucursal == idSucursal &&
                        u.Activo);
            }

        public async Task<int> ContarProfesionalesActivosPorEspecialidadAsync(int idClinica, int idSucursal, int idEspecialidad)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .CountAsync(u =>
                    u.IdClinica == idClinica &&
                    u.IdSucursal == idSucursal &&
                    u.IdEspecialidad == idEspecialidad &&
                    u.Activo &&
                    u.Rol != null &&
                    u.Rol.Activo &&
                    u.Rol.Nombre != null &&
                    u.Rol.Nombre.ToLower() == "doctor");
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .Include(u => u.Especialidad)
                .FirstOrDefaultAsync(u => u.Id == id);

            return await AdjuntarRolSiAplicaAsync(usuario);
        }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return null;

            var usuario = await _context.Usuarios
                .Include(u => u.Clinica)
                .Include(u => u.Sucursal)
                .Include(u => u.Especialidad)
                .FirstOrDefaultAsync(u => u.Correo.ToLower() == correo.ToLower());

            return await AdjuntarRolSiAplicaAsync(usuario);
        }

        private async Task<Usuario?> AdjuntarRolSiAplicaAsync(Usuario? usuario)
        {
            if (usuario == null)
            {
                return null;
            }

            usuario.Rol = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == usuario.IdRol && r.Activo);

            return usuario;
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

        public async Task<bool> ExisteCorreoPorClinicaAsync(int idClinica, string correo, int? idExcluir = null)
        {
            var query = _context.Usuarios
                .Where(u => u.IdClinica == idClinica && u.Correo.ToLower() == correo.ToLower());

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
