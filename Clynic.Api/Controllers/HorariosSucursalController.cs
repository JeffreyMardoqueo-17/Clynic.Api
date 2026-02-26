using Clynic.Application.DTOs.HorariosSucursal;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Clynic.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar la configuracion de horarios por sucursal
    /// </summary>
    [ApiController]
    [Route("/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class HorariosSucursalController : ControllerBase
    {
        private readonly IHorarioSucursalService _horarioSucursalService;

        public HorariosSucursalController(IHorarioSucursalService horarioSucursalService)
        {
            _horarioSucursalService = horarioSucursalService ?? throw new ArgumentNullException(nameof(horarioSucursalService));
        }

        /// <summary>
        /// Obtiene todos los horarios
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IEnumerable<HorarioSucursalResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<HorarioSucursalResponseDto>>> ObtenerTodos()
        {
            var horarios = await _horarioSucursalService.ObtenerTodosAsync();
            return Ok(horarios);
        }

        /// <summary>
        /// Obtiene un horario por su ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(HorarioSucursalResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HorarioSucursalResponseDto>> ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            var horario = await _horarioSucursalService.ObtenerPorIdAsync(id);

            if (horario == null)
            {
                return NotFound(new { mensaje = $"No se encontro el horario con ID {id}" });
            }

            return Ok(horario);
        }

        /// <summary>
        /// Obtiene los horarios de una sucursal
        /// </summary>
        [HttpGet("sucursal/{idSucursal}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IEnumerable<HorarioSucursalResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<HorarioSucursalResponseDto>>> ObtenerPorSucursal(int idSucursal)
        {
            if (idSucursal <= 0)
            {
                return BadRequest(new { mensaje = "El ID de la sucursal debe ser mayor a cero" });
            }

            var horarios = await _horarioSucursalService.ObtenerPorSucursalAsync(idSucursal);
            return Ok(horarios);
        }

        /// <summary>
        /// Crea una nueva configuracion de horario para sucursal
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(HorarioSucursalResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HorarioSucursalResponseDto>> Crear([FromBody] CreateHorarioSucursalDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { mensaje = "Los datos del horario son requeridos" });
            }

            var horarioCreado = await _horarioSucursalService.CrearAsync(createDto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = horarioCreado.Id },
                horarioCreado);
        }

        /// <summary>
        /// Actualiza una configuracion de horario existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(HorarioSucursalResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<HorarioSucursalResponseDto>> Actualizar(int id, [FromBody] UpdateHorarioSucursalDto updateDto)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            if (updateDto == null)
            {
                return BadRequest(new { mensaje = "Los datos del horario son requeridos" });
            }

            try
            {
                var horarioActualizado = await _horarioSucursalService.ActualizarAsync(id, updateDto);
                return Ok(horarioActualizado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una configuracion de horario existente
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Eliminar(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            var eliminado = await _horarioSucursalService.EliminarAsync(id);
            if (!eliminado)
            {
                return NotFound(new { mensaje = $"No se encontro el horario con ID {id}" });
            }

            return NoContent();
        }

        /// <summary>
        /// Obtiene los asuetos de una sucursal
        /// </summary>
        [HttpGet("sucursal/{idSucursal}/asuetos")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IEnumerable<AsuetoSucursalResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AsuetoSucursalResponseDto>>> ObtenerAsuetosPorSucursal(int idSucursal)
        {
            if (idSucursal <= 0)
            {
                return BadRequest(new { mensaje = "El ID de la sucursal debe ser mayor a cero" });
            }

            var asuetos = await _horarioSucursalService.ObtenerAsuetosPorSucursalAsync(idSucursal);
            return Ok(asuetos);
        }

        /// <summary>
        /// Crea un asueto de sucursal
        /// </summary>
        [HttpPost("asuetos")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AsuetoSucursalResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AsuetoSucursalResponseDto>> CrearAsueto([FromBody] CreateAsuetoSucursalDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { mensaje = "Los datos del asueto son requeridos" });
            }

            var asuetoCreado = await _horarioSucursalService.CrearAsuetoAsync(createDto);

            return CreatedAtAction(
                nameof(ObtenerAsuetosPorSucursal),
                new { idSucursal = asuetoCreado.IdSucursal },
                asuetoCreado);
        }

        /// <summary>
        /// Elimina un asueto por ID
        /// </summary>
        [HttpDelete("asuetos/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> EliminarAsueto(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            var eliminado = await _horarioSucursalService.EliminarAsuetoAsync(id);
            if (!eliminado)
            {
                return NotFound(new { mensaje = $"No se encontro el asueto con ID {id}" });
            }

            return NoContent();
        }
    }
}
