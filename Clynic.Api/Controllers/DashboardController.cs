using Clynic.Application.DTOs.Dashboard;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,Doctor,Recepcionista")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        [HttpGet("resumen")]
        [ProducesResponseType(typeof(DashboardResumenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DashboardResumenDto>> ObtenerResumen([FromQuery] int? idSucursal = null)
        {
            if (!TryGetIdClinicaToken(out var idClinica))
            {
                return Forbid();
            }

            var idSucursalScope = ResolveSucursalScope(idSucursal);
            var result = await _dashboardService.ObtenerResumenAsync(idClinica, idSucursalScope);
            return Ok(result);
        }

        [HttpGet("citas-por-dia")]
        [ProducesResponseType(typeof(DashboardCitasSerieDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DashboardCitasSerieDto>> ObtenerCitasPorDia(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string? periodo = null,
            [FromQuery] int? idSucursal = null)
        {
            if (!TryGetIdClinicaToken(out var idClinica))
            {
                return Forbid();
            }

            var hoy = DateTime.UtcNow.Date;

            if (!fechaDesde.HasValue && !fechaHasta.HasValue && !string.IsNullOrWhiteSpace(periodo))
            {
                var normalized = periodo.Trim().ToLowerInvariant();
                (fechaDesde, fechaHasta) = normalized switch
                {
                    "semanal" => (hoy.AddDays(-5), hoy.AddDays(1)),
                    "mensual" => (hoy.AddDays(-28), hoy.AddDays(1)),
                    "anual" => (hoy.AddDays(-363), hoy.AddDays(1)),
                    "todo" => (new DateTime(1970, 1, 1), hoy.AddDays(1)),
                    _ => throw new ArgumentException("Periodo inválido. Usa: semanal, mensual, anual o todo.")
                };
            }

            var idSucursalScope = ResolveSucursalScope(idSucursal);
            var result = await _dashboardService.ObtenerCitasPorDiaAsync(idClinica, fechaDesde, fechaHasta, idSucursalScope);
            return Ok(result);
        }

        [HttpGet("operativo")]
        [ProducesResponseType(typeof(DashboardOperativoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DashboardOperativoDto>> ObtenerOperativo([FromQuery] int? idSucursal = null)
        {
            if (!TryGetIdClinicaToken(out var idClinica))
            {
                return Forbid();
            }

            var idSucursalScope = ResolveSucursalScope(idSucursal);
            var result = await _dashboardService.ObtenerOperativoAsync(idClinica, idSucursalScope);
            return Ok(result);
        }

        private bool TryGetIdClinicaToken(out int idClinica)
        {
            var claim = User.FindFirst("IdClinica")?.Value;
            return int.TryParse(claim, out idClinica) && idClinica > 0;
        }

        private bool TryGetIdSucursalToken(out int idSucursal)
        {
            var claim = User.FindFirst("IdSucursal")?.Value;
            return int.TryParse(claim, out idSucursal) && idSucursal > 0;
        }

        private int? ResolveSucursalScope(int? requestedIdSucursal)
        {
            if (User.IsInRole("Admin"))
            {
                return requestedIdSucursal.HasValue && requestedIdSucursal.Value > 0
                    ? requestedIdSucursal.Value
                    : null;
            }

            if (TryGetIdSucursalToken(out var idSucursal))
            {
                return idSucursal;
            }

            return null;
        }
    }
}
