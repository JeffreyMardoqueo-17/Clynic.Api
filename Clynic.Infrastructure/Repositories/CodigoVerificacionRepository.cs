using Microsoft.EntityFrameworkCore;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;

namespace Clynic.Infrastructure.Repositories
{
    public class CodigoVerificacionRepository : ICodigoVerificacionRepository
    {
        private readonly ClynicDbContext _context;

        public CodigoVerificacionRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CodigoVerificacion?> ObtenerCodigoValidoAsync(int idUsuario, string codigo, TipoCodigo tipo)
        {
            return await _context.CodigosVerificacion
                .FirstOrDefaultAsync(c => 
                    c.IdUsuario == idUsuario &&
                    c.Codigo == codigo &&
                    c.Tipo == tipo &&
                    !c.Usado &&
                    c.FechaExpiracion > DateTime.UtcNow);
        }

        public async Task<CodigoVerificacion> CrearAsync(CodigoVerificacion codigo)
        {
            if (codigo == null)
                throw new ArgumentNullException(nameof(codigo));

            await _context.CodigosVerificacion.AddAsync(codigo);
            await _context.SaveChangesAsync();

            return codigo;
        }

        public async Task MarcarComoUsadoAsync(CodigoVerificacion codigo)
        {
            if (codigo == null)
                throw new ArgumentNullException(nameof(codigo));

            codigo.Usado = true;
            codigo.FechaUso = DateTime.UtcNow;

            _context.CodigosVerificacion.Update(codigo);
            await _context.SaveChangesAsync();
        }

        public async Task InvalidarCodigosAnterioresAsync(int idUsuario, TipoCodigo tipo)
        {
            var codigos = await _context.CodigosVerificacion
                .Where(c => c.IdUsuario == idUsuario && c.Tipo == tipo && !c.Usado && c.FechaExpiracion > DateTime.UtcNow)
                .ToListAsync();

            foreach (var codigo in codigos)
            {
                codigo.Usado = true;
                codigo.FechaUso = DateTime.UtcNow;
            }

            if (codigos.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
