using Clynic.Application.DTOs.CitaServicios;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin,Doctor,Recepcionista")]
    public class CitaServiciosController : ControllerBase
    {
        private readonly ICitaServicioService _citaServicioService;
        private readonly ICitaService _citaService;

        public CitaServiciosController(
            ICitaServicioService citaServicioService,
            ICitaService citaService)
        {
            _citaServicioService = citaServicioService ?? throw new ArgumentNullException(nameof(citaServicioService));
            _citaService = citaService ?? throw new ArgumentNullException(nameof(citaService));
        }

        [HttpGet("cita/{idCita}")]
        [ProducesResponseType(typeof(IReadOnlyCollection<CitaServicioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<CitaServicioResponseDto>>> ObtenerPorCita(int idCita)
        {
            var cita = await _citaService.ObtenerPorIdAsync(idCita);
            if (cita == null)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ la cita con ID {idCita}" });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != cita.IdClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetIdSucursalToken(out var idSucursalToken) || idSucursalToken != cita.IdSucursal)
                {
                    return Forbid();
                }
            }

            var detalles = await _citaServicioService.ObtenerPorCitaAsync(idCita);
            return Ok(detalles);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Recepcionista")]
        [ProducesResponseType(typeof(CitaServicioResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CitaServicioResponseDto>> Crear([FromBody] CreateCitaServicioDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos del detalle de cita son requeridos." });
            }

            var cita = await _citaService.ObtenerPorIdAsync(dto.IdCita);
            if (cita == null)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ la cita con ID {dto.IdCita}" });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != cita.IdClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetIdSucursalToken(out var idSucursalToken) || idSucursalToken != cita.IdSucursal)
                {
                    return Forbid();
                }
            }

            var creado = await _citaServicioService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorCita), new { idCita = creado.IdCita }, creado);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Recepcionista")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Eliminar(int id)
        {
            var detalle = await _citaServicioService.ObtenerPorIdAsync(id);
            if (detalle == null)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ el detalle de cita con ID {id}" });
            }

            var cita = await _citaService.ObtenerPorIdAsync(detalle.IdCita);
            if (cita == null)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ la cita con ID {detalle.IdCita}" });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != cita.IdClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetIdSucursalToken(out var idSucursalToken) || idSucursalToken != cita.IdSucursal)
                {
                    return Forbid();
                }
            }

            var eliminado = await _citaServicioService.EliminarAsync(id);
            if (!eliminado)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ el detalle de cita con ID {id}" });
            }

            return NoContent();
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
    }
}

