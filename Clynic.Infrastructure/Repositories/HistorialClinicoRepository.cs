using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class HistorialClinicoRepository : IHistorialClinicoRepository
    {
        private readonly ClynicDbContext _context;

        public HistorialClinicoRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<HistorialClinico?> ObtenerPorPacienteAsync(int idPaciente)
        {
            return await _context.HistorialesClinicos
                .FirstOrDefaultAsync(h => h.IdPaciente == idPaciente);
        }

        public async Task<HistorialClinico> GuardarAsync(HistorialClinico historialClinico)
        {
            if (historialClinico == null)
            {
                throw new ArgumentNullException(nameof(historialClinico));
            }

            var existente = await _context.HistorialesClinicos
                .FirstOrDefaultAsync(h => h.IdPaciente == historialClinico.IdPaciente);

            if (existente == null)
            {
                historialClinico.FechaCreacion = DateTime.UtcNow;
                historialClinico.FechaActualizacion = DateTime.UtcNow;
                await _context.HistorialesClinicos.AddAsync(historialClinico);
            }
            else
            {
                existente.EnfermedadesPrevias = historialClinico.EnfermedadesPrevias;
                existente.MedicamentosActuales = historialClinico.MedicamentosActuales;
                existente.Alergias = historialClinico.Alergias;
                existente.AntecedentesFamiliares = historialClinico.AntecedentesFamiliares;
                existente.Observaciones = historialClinico.Observaciones;
                existente.FechaActualizacion = DateTime.UtcNow;
                historialClinico = existente;
            }

            await _context.SaveChangesAsync();
            return historialClinico;
        }
    }
}
