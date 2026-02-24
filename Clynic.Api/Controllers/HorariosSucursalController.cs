using Clynic.Application.DTOs.HorariosSucursal;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar la configuracion de horarios por sucursal
    /// </summary>
    [ApiController]
    [Route("/[controller]")]
    [Produces("application/json")]
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
    }
}
