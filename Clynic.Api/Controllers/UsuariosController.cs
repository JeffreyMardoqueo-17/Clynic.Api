using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models.Enums;
using System.Security.Claims;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> ObtenerTodos()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            return Ok(usuarios);
        }

        [HttpGet("clinica/{idClinica}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> ObtenerPorClinica(int idClinica)
        {
            var usuarios = await _usuarioService.ObtenerPorClinicaAsync(idClinica);
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            var usuario = await _usuarioService.ObtenerPorIdAsync(id);

            if (usuario == null)
            {
                return NotFound(new { mensaje = $"No se encontró el usuario con ID {id}" });
            }

            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rol != "Admin" && usuarioId != id.ToString())
            {
                return Forbid();
            }

            return Ok(usuario);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioResponseDto>> Crear([FromBody] RegisterDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { mensaje = "Los datos del usuario son requeridos" });
            }

            var usuarioCreado = await _usuarioService.CrearAsync(createDto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = usuarioCreado.Id },
                usuarioCreado);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UsuarioResponseDto>> Actualizar(int id, [FromBody] UpdateUsuarioDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            if (updateDto == null)
            {
                return BadRequest(new { mensaje = "Los datos de actualización son requeridos" });
            }

            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rol != "Admin" && usuarioId != id.ToString())
            {
                return Forbid();
            }

            var resultado = await _usuarioService.ActualizarAsync(id, updateDto);

            if (resultado == null)
            {
                return NotFound(new { mensaje = $"No se encontró el usuario con ID {id}" });
            }

            return Ok(resultado);
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

            var eliminado = await _usuarioService.EliminarAsync(id);

            if (!eliminado)
            {
                return NotFound(new { mensaje = $"No se encontró el usuario con ID {id}" });
            }

            return NoContent();
        }

        [HttpPut("{id}/cambiar-clave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CambiarClave(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            if (changePasswordDto == null)
            {
                return BadRequest(new { mensaje = "Los datos de cambio de clave son requeridos" });
            }

            var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;

            if (rol != "Admin" && usuarioId != id.ToString())
            {
                return Forbid();
            }

            try
            {
                var resultado = await _usuarioService.CambiarClaveAsync(id, changePasswordDto);

                if (!resultado)
                {
                    return NotFound(new { mensaje = $"No se encontró el usuario con ID {id}" });
                }

                return Ok(new { mensaje = "Clave actualizada exitosamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
