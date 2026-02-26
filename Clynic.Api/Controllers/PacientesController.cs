using Clynic.Application.DTOs.Pacientes;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,Doctor,Recepcionista")]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;

        public PacientesController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService ?? throw new ArgumentNullException(nameof(pacienteService));
        }

        [HttpGet("clinica/{idClinica}")]
        [ProducesResponseType(typeof(IEnumerable<PacienteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PacienteResponseDto>>> ObtenerPorClinica(int idClinica, [FromQuery] string? busqueda = null)
        {
            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != idClinica)
            {
                return Forbid();
            }

            var pacientes = await _pacienteService.ObtenerPorClinicaAsync(idClinica, busqueda);
            return Ok(pacientes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PacienteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PacienteResponseDto>> ObtenerPorId(int id)
        {
            var paciente = await _pacienteService.ObtenerPorIdAsync(id);
            if (paciente == null)
            {
                return NotFound(new { mensaje = $"No se encontró el paciente con ID {id}" });
            }

            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != paciente.IdClinica)
            {
                return Forbid();
            }

            return Ok(paciente);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PacienteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PacienteResponseDto>> Actualizar(int id, [FromBody] UpdatePacienteDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos del paciente son requeridos." });
            }

            var pacienteExistente = await _pacienteService.ObtenerPorIdAsync(id);
            if (pacienteExistente == null)
            {
                return NotFound(new { mensaje = $"No se encontró el paciente con ID {id}" });
            }

            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != pacienteExistente.IdClinica)
            {
                return Forbid();
            }

            var actualizado = await _pacienteService.ActualizarAsync(id, dto);
            if (actualizado == null)
            {
                return NotFound(new { mensaje = $"No se encontró el paciente con ID {id}" });
            }

            return Ok(actualizado);
        }

        [HttpPut("{id}/historial")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(HistorialClinicoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HistorialClinicoResponseDto>> GuardarHistorial(int id, [FromBody] UpdateHistorialClinicoDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos del historial son requeridos." });
            }

            var pacienteExistente = await _pacienteService.ObtenerPorIdAsync(id);
            if (pacienteExistente == null)
            {
                return NotFound(new { mensaje = $"No se encontró el paciente con ID {id}" });
            }

            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != pacienteExistente.IdClinica)
            {
                return Forbid();
            }

            var historial = await _pacienteService.GuardarHistorialAsync(id, dto);
            return Ok(historial);
        }
    }
}
