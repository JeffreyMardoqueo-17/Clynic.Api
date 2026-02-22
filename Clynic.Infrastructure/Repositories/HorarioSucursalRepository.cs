using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    /// <summary>
    /// Implementacion del repositorio de HorarioSucursal usando Entity Framework Core
    /// </summary>
    public class HorarioSucursalRepository : IHorarioSucursalRepository
    {
        private readonly ClynicDbContext _context;

        public HorarioSucursalRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<HorarioSucursal>> ObtenerTodosAsync()
        {
            return await _context.HorariosSucursal
                .OrderBy(h => h.IdSucursal)
                .ThenBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task<IEnumerable<HorarioSucursal>> ObtenerPorSucursalAsync(int idSucursal)
        {
            return await _context.HorariosSucursal
                .Where(h => h.IdSucursal == idSucursal)
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task<HorarioSucursal?> ObtenerPorIdAsync(int id)
        {
            return await _context.HorariosSucursal
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<HorarioSucursal> CrearAsync(HorarioSucursal horario)
        {
            if (horario == null)
                throw new ArgumentNullException(nameof(horario));

            await _context.HorariosSucursal.AddAsync(horario);
            await _context.SaveChangesAsync();

            return horario;
        }

        public async Task<bool> ExisteCruceHorarioAsync(int idSucursal, int diaSemana, TimeSpan horaInicio, TimeSpan horaFin)
        {
            return await _context.HorariosSucursal
                .AnyAsync(h =>
                    h.IdSucursal == idSucursal &&
                    h.DiaSemana == diaSemana &&
                    h.HoraInicio.HasValue &&
                    h.HoraFin.HasValue &&
                    h.HoraInicio.Value < horaFin &&
                    h.HoraFin.Value > horaInicio);
        }
    }
}
