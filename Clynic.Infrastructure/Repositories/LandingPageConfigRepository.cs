using Clynic.Application.Interfaces.Repositories;
using Clynic.Domain.Models;
using Clynic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Repositories
{
    public class LandingPageConfigRepository : ILandingPageConfigRepository
    {
        private readonly ClynicDbContext _context;

        public LandingPageConfigRepository(ClynicDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<LandingPageConfig?> ObtenerPorClinicaAsync(int idClinica)
        {
            return await _context.LandingPageConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdClinica == idClinica);
        }

        public async Task<LandingPageConfig?> ObtenerPublicadaPorDominioAsync(string dominioBase)
        {
            return await _context.LandingPageConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Publicada && x.DominioBase == dominioBase);
        }

        public async Task<LandingPageConfig?> ObtenerCualquierPublicadaAsync()
        {
            return await _context.LandingPageConfigs
                .AsNoTracking()
                .Where(x => x.Publicada)
                .OrderBy(x => x.IdClinica)
                .FirstOrDefaultAsync();
        }

        public async Task<LandingPageConfig> CrearAsync(LandingPageConfig config)
        {
            await _context.LandingPageConfigs.AddAsync(config);
            await _context.SaveChangesAsync();
            return config;
        }

        public async Task<LandingPageConfig> ActualizarAsync(LandingPageConfig config)
        {
            _context.LandingPageConfigs.Update(config);
            await _context.SaveChangesAsync();
            return config;
        }
    }
}
