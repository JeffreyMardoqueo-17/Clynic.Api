using Clynic.Application.DTOs.Citas;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CitasController : ControllerBase
    {
        private readonly ICitaService _citaService;

        public CitasController(ICitaService citaService)
        {
            _citaService = citaService ?? throw new ArgumentNullException(nameof(citaService));
        }

        [HttpGet("publica/catalogo/{idClinica}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CatalogoCitaPublicaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CatalogoCitaPublicaDto>> ObtenerCatalogoPublico(int idClinica)
        {
            if (idClinica <= 0)
            {
                return BadRequest(new { mensaje = "El ID de clínica debe ser mayor a cero." });
            }

            var catalogo = await _citaService.ObtenerCatalogoPublicoAsync(idClinica);
            return Ok(catalogo);
        }

        [HttpPost("publica")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CitaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CitaResponseDto>> CrearPublica([FromBody] CreateCitaPublicaDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos de la cita son requeridos." });
            }

            var creada = await _citaService.CrearPublicaAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
        }

        [HttpPost("interna")]
        [Authorize(Roles = "Admin,Recepcionista")]
        [ProducesResponseType(typeof(CitaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CitaResponseDto>> CrearInterna([FromBody] CreateCitaInternaDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos de la cita son requeridos." });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != dto.IdClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetIdSucursalToken(out var idSucursalToken) || idSucursalToken != dto.IdSucursal)
                {
                    return Forbid();
                }
            }

            var creada = await _citaService.CrearInternaAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
        }

        [HttpGet("clinica/{idClinica}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IEnumerable<CitaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CitaResponseDto>>> ObtenerPorClinica(
            int idClinica,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? idSucursal = null,
            [FromQuery] EstadoCita? estado = null)
        {
            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != idClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetIdSucursalToken(out var idSucursalToken))
                {
                    return Forbid();
                }

                idSucursal = idSucursalToken;
            }

            var citas = await _citaService.ObtenerPorClinicaAsync(idClinica, fechaDesde, fechaHasta, idSucursal, estado);
            return Ok(citas);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(CitaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CitaResponseDto>> ObtenerPorId(int id)
        {
            var cita = await _citaService.ObtenerPorIdAsync(id);
            if (cita == null)
            {
                return NotFound(new { mensaje = $"No se encontró la cita con ID {id}" });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != cita.IdClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetIdSucursalToken(out var idSucursalToken) || cita.IdSucursal != idSucursalToken)
                {
                    return Forbid();
                }
            }

            return Ok(cita);
        }

        [HttpPut("{id}/doctor")]
        [Authorize(Roles = "Admin,Recepcionista")]
        [ProducesResponseType(typeof(CitaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CitaResponseDto>> AsignarDoctor(int id, [FromBody] AsignarDoctorCitaDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos son requeridos." });
            }

            var cita = await _citaService.ObtenerPorIdAsync(id);
            if (cita == null)
            {
                return NotFound(new { mensaje = $"No se encontró la cita con ID {id}" });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != cita.IdClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetIdSucursalToken(out var idSucursalToken) || cita.IdSucursal != idSucursalToken)
                {
                    return Forbid();
                }
            }

            var actualizada = await _citaService.AsignarDoctorAsync(id, dto);
            if (actualizada == null)
            {
                return NotFound(new { mensaje = $"No se encontró la cita con ID {id}" });
            }

            return Ok(actualizada);
        }

        [HttpPost("{id}/consulta")]
        [Authorize(Roles = "Admin,Doctor")]
        [ProducesResponseType(typeof(ConsultaMedicaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConsultaMedicaResponseDto>> RegistrarConsulta(int id, [FromBody] RegistrarConsultaMedicaDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos de la consulta son requeridos." });
            }

            var cita = await _citaService.ObtenerPorIdAsync(id);
            if (cita == null)
            {
                return NotFound(new { mensaje = $"No se encontró la cita con ID {id}" });
            }

            if (!TryGetIdClinicaToken(out var idClinicaToken) || idClinicaToken != cita.IdClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                if (!TryGetIdSucursalToken(out var idSucursalToken) || cita.IdSucursal != idSucursalToken)
                {
                    return Forbid();
                }
            }

            var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuario, out var idDoctorEjecutor) || idDoctorEjecutor <= 0)
            {
                return Unauthorized(new { mensaje = "No se pudo identificar el usuario autenticado." });
            }

            var consulta = await _citaService.RegistrarConsultaAsync(id, idDoctorEjecutor, dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, consulta);
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
