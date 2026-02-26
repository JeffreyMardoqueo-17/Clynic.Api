using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class PacienteRepository : IPacienteRepository
    {
        private readonly ClynicDbContext _context;

        public PacienteRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Paciente?> ObtenerPorIdAsync(int id)
        {
            return await _context.Pacientes
                .Include(p => p.HistorialClinico)
                .Include(p => p.ConsultasMedicas)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Paciente?> ObtenerPorCorreoAsync(int idClinica, string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                return null;
            }

            var correoNormalizado = correo.Trim().ToLower();
            return await _context.Pacientes
                .Include(p => p.HistorialClinico)
                .FirstOrDefaultAsync(p => p.IdClinica == idClinica && p.Correo.ToLower() == correoNormalizado);
        }

        public async Task<IEnumerable<Paciente>> ObtenerPorClinicaAsync(int idClinica, string? busqueda = null)
        {
            var query = _context.Pacientes
                .Include(p => p.HistorialClinico)
                .Include(p => p.ConsultasMedicas)
                .Where(p => p.IdClinica == idClinica);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim().ToLower();
                query = query.Where(p =>
                    p.Nombres.ToLower().Contains(termino) ||
                    p.Apellidos.ToLower().Contains(termino) ||
                    p.Correo.ToLower().Contains(termino) ||
                    p.Telefono.ToLower().Contains(termino));
            }

            return await query
                .OrderBy(p => p.Apellidos)
                .ThenBy(p => p.Nombres)
                .ToListAsync();
        }

        public async Task<Paciente> CrearAsync(Paciente paciente)
        {
            if (paciente == null)
            {
                throw new ArgumentNullException(nameof(paciente));
            }

            paciente.FechaRegistro = DateTime.UtcNow;

            await _context.Pacientes.AddAsync(paciente);
            await _context.SaveChangesAsync();

            return paciente;
        }

        public async Task<Paciente> ActualizarAsync(Paciente paciente)
        {
            if (paciente == null)
            {
                throw new ArgumentNullException(nameof(paciente));
            }

            _context.Pacientes.Update(paciente);
            await _context.SaveChangesAsync();

            return paciente;
        }

        public async Task<HistorialClinico?> ObtenerHistorialPorPacienteAsync(int idPaciente)
        {
            return await _context.HistorialesClinicos
                .FirstOrDefaultAsync(h => h.IdPaciente == idPaciente);
        }

        public async Task<HistorialClinico> GuardarHistorialAsync(HistorialClinico historial)
        {
            if (historial == null)
            {
                throw new ArgumentNullException(nameof(historial));
            }

            var existente = await _context.HistorialesClinicos
                .FirstOrDefaultAsync(h => h.IdPaciente == historial.IdPaciente);

            if (existente == null)
            {
                historial.FechaCreacion = DateTime.UtcNow;
                historial.FechaActualizacion = DateTime.UtcNow;
                await _context.HistorialesClinicos.AddAsync(historial);
            }
            else
            {
                existente.EnfermedadesPrevias = historial.EnfermedadesPrevias;
                existente.MedicamentosActuales = historial.MedicamentosActuales;
                existente.Alergias = historial.Alergias;
                existente.AntecedentesFamiliares = historial.AntecedentesFamiliares;
                existente.Observaciones = historial.Observaciones;
                existente.FechaActualizacion = DateTime.UtcNow;
                historial = existente;
            }

            await _context.SaveChangesAsync();
            return historial;
        }
    }
}
