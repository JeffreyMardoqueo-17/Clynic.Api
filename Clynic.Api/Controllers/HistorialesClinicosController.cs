using Clynic.Application.DTOs.HistorialesClinicos;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,Doctor,Nutricionista,Fisioterapeuta")]
    public class HistorialesClinicosController : ControllerBase
    {
        private readonly IHistorialClinicoService _historialClinicoService;
        private readonly IPacienteService _pacienteService;

        public HistorialesClinicosController(
            IHistorialClinicoService historialClinicoService,
            IPacienteService pacienteService)
        {
            _historialClinicoService = historialClinicoService ?? throw new ArgumentNullException(nameof(historialClinicoService));
            _pacienteService = pacienteService ?? throw new ArgumentNullException(nameof(pacienteService));
        }

        [HttpGet("paciente/{idPaciente}")]
        [ProducesResponseType(typeof(HistorialClinicoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HistorialClinicoResponseDto>> ObtenerPorPaciente(int idPaciente)
        {
            var paciente = await _pacienteService.ObtenerPorIdAsync(idPaciente);
            if (paciente == null)
            {
                return NotFound(new { mensaje = $"No se encontró el paciente con ID {idPaciente}" });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != paciente.IdClinica)
            {
                return Forbid();
            }

            var historial = await _historialClinicoService.ObtenerPorPacienteAsync(idPaciente);
            if (historial == null)
            {
                return NotFound(new { mensaje = $"No se encontró historial clínico para el paciente {idPaciente}" });
            }

            return Ok(historial);
        }

        [HttpPut("paciente/{idPaciente}")]
        [ProducesResponseType(typeof(HistorialClinicoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HistorialClinicoResponseDto>> Guardar(int idPaciente, [FromBody] UpsertHistorialClinicoDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos del historial clínico son requeridos." });
            }

            var paciente = await _pacienteService.ObtenerPorIdAsync(idPaciente);
            if (paciente == null)
            {
                return NotFound(new { mensaje = $"No se encontró el paciente con ID {idPaciente}" });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != paciente.IdClinica)
            {
                return Forbid();
            }

            var historial = await _historialClinicoService.GuardarAsync(idPaciente, dto);
            return Ok(historial);
        }

        private bool TryGetIdClinicaToken(out int idClinica)
        {
            var claim = User.FindFirst("IdClinica")?.Value;
            return int.TryParse(claim, out idClinica) && idClinica > 0;
        }
    }
}
