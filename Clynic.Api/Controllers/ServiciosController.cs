using Clynic.Application.DTOs.Servicios;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ServiciosController : ControllerBase
    {
        private readonly IServicioService _servicioService;

        public ServiciosController(IServicioService servicioService)
        {
            _servicioService = servicioService ?? throw new ArgumentNullException(nameof(servicioService));
        }

        [HttpGet("clinica/{idClinica}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IEnumerable<ServicioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ServicioResponseDto>>> ObtenerPorClinica(
            int idClinica,
            [FromQuery] string? nombre = null,
            [FromQuery] bool incluirInactivos = false)
        {
            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != idClinica)
            {
                return Forbid();
            }

            var servicios = await _servicioService.ObtenerPorClinicaAsync(idClinica, nombre, incluirInactivos);
            return Ok(servicios);
        }

        [HttpGet("clinica/{idClinica}/capacidad-especialidad")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IReadOnlyCollection<CapacidadEspecialidadDiaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyCollection<CapacidadEspecialidadDiaDto>>> ObtenerCapacidadPorEspecialidad(
            int idClinica,
            [FromQuery] DateTime fechaDesde,
            [FromQuery] DateTime fechaHasta,
            [FromQuery] int? idSucursal = null,
            [FromQuery] int horasLaborablesDia = 8,
            [FromQuery] int minutosAlmuerzoDia = 60)
        {
            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != idClinica)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                var idSucursalClaim = User.FindFirst("IdSucursal")?.Value;
                if (!int.TryParse(idSucursalClaim, out var idSucursalToken))
                {
                    return Forbid();
                }

                if (idSucursal.HasValue && idSucursal.Value != idSucursalToken)
                {
                    return Forbid();
                }

                idSucursal = idSucursalToken;
            }

            var data = await _servicioService.ObtenerCapacidadPorEspecialidadAsync(
                idClinica,
                fechaDesde,
                fechaHasta,
                idSucursal,
                horasLaborablesDia,
                minutosAlmuerzoDia);

            return Ok(data);
        }

        [HttpGet("publicos/clinica/{idClinica}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ServicioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ServicioResponseDto>>> ObtenerPublicosPorClinica(
            int idClinica,
            [FromQuery] string? nombre = null)
        {
            if (idClinica <= 0)
            {
                return BadRequest(new { mensaje = "El ID de clÃ­nica debe ser mayor a cero." });
            }

            var servicios = await _servicioService.ObtenerPorClinicaAsync(idClinica, nombre, incluirInactivos: false);
            return Ok(servicios);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(ServicioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServicioResponseDto>> ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            var servicio = await _servicioService.ObtenerPorIdAsync(id, incluirInactivos: true);
            if (servicio == null)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ el servicio con ID {id}" });
            }

            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != servicio.IdClinica)
            {
                return Forbid();
            }

            return Ok(servicio);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServicioResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServicioResponseDto>> Crear([FromBody] CreateServicioDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { mensaje = "Los datos del servicio son requeridos" });
            }

            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != createDto.IdClinica)
            {
                return Forbid();
            }

            var servicioCreado = await _servicioService.CrearAsync(createDto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = servicioCreado.Id },
                servicioCreado);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServicioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServicioResponseDto>> Actualizar(int id, [FromBody] UpdateServicioDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            if (updateDto == null)
            {
                return BadRequest(new { mensaje = "Los datos de actualizaciÃ³n son requeridos" });
            }

            var servicioExistente = await _servicioService.ObtenerPorIdAsync(id, incluirInactivos: true);
            if (servicioExistente == null)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ el servicio con ID {id}" });
            }

            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != servicioExistente.IdClinica)
            {
                return Forbid();
            }

            var actualizado = await _servicioService.ActualizarAsync(id, updateDto);
            if (actualizado == null)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ el servicio con ID {id}" });
            }

            return Ok(actualizado);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            var servicioExistente = await _servicioService.ObtenerPorIdAsync(id, incluirInactivos: true);
            if (servicioExistente == null)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ el servicio con ID {id}" });
            }

            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            if (!int.TryParse(idClinicaClaim, out var idClinicaToken) || idClinicaToken != servicioExistente.IdClinica)
            {
                return Forbid();
            }

            var eliminado = await _servicioService.EliminarAsync(id);
            if (!eliminado)
            {
                return NotFound(new { mensaje = $"No se encontrÃ³ el servicio con ID {id}" });
            }

            return NoContent();
        }
    }
}

