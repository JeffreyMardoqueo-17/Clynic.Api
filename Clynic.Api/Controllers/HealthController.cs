using Microsoft.AspNetCore.Mvc;
using Clynic.Infrastructure.Data;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ClynicDbContext _dbContext;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ClynicDbContext dbContext, ILogger<HealthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Verificar salud de la API
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                // Probar conexión a BD
                await _dbContext.Database.CanConnectAsync();
                
                return Ok(new
                {
                    status = "✅ OK",
                    timestamp = DateTime.UtcNow,
                    environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    database = "🗄️ Conectada"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en health check");
                return StatusCode(500, new
                {
                    status = "❌ Error",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener versión de la API
        /// </summary>
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            return Ok(new
            {
                version = "1.0.0",
                name = "Clynic API",
                description = "Sistema de citas para clínicas"
            });
        }
    }
}
