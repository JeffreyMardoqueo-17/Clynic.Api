using Microsoft.AspNetCore.Mvc;
using Clynic.Application.DTOs.Sucursales;
using Clynic.Application.Interfaces.Services;
using FluentValidation;

namespace Clynic.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar las Sucursales
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SucursalesController : ControllerBase
    {
        private readonly ISucursalService _sucursalService;
        private readonly ILogger<SucursalesController> _logger;

        public SucursalesController(
            ISucursalService sucursalService,
            ILogger<SucursalesController> logger)
        {
            _sucursalService = sucursalService ?? throw new ArgumentNullException(nameof(sucursalService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene todas las sucursales activas
        /// </summary>
        /// <returns>Lista de sucursales</returns>
        /// <response code="200">Retorna la lista de sucursales</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SucursalResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SucursalResponseDto>>> ObtenerTodas()
        {
            try
            {
                var sucursales = await _sucursalService.ObtenerTodasAsync();
                return Ok(sucursales);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las sucursales");
                return StatusCode(500, new { mensaje = "Error al obtener las sucursales", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una sucursal por su ID
        /// </summary>
        /// <param name="id">ID de la sucursal</param>
        /// <returns>La sucursal solicitada</returns>
        /// <response code="200">Retorna la sucursal encontrada</response>
        /// <response code="404">No se encontro la sucursal</response>
        /// <response code="400">ID invalido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SucursalResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SucursalResponseDto>> ObtenerPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
                }

                var sucursal = await _sucursalService.ObtenerPorIdAsync(id);

                if (sucursal == null)
                {
                    return NotFound(new { mensaje = $"No se encontro la sucursal con ID {id}" });
                }

                return Ok(sucursal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la sucursal con ID {Id}", id);
                return StatusCode(500, new { mensaje = "Error al obtener la sucursal", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva sucursal
        /// </summary>
        /// <param name="createDto">Datos de la nueva sucursal</param>
        /// <returns>La sucursal creada</returns>
        /// <response code="201">Sucursal creada exitosamente</response>
        /// <response code="400">Datos de entrada invalidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(SucursalResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SucursalResponseDto>> Crear([FromBody] CreateSucursalDto createDto)
        {
            try
            {
                if (createDto == null)
                {
                    return BadRequest(new { mensaje = "Los datos de la sucursal son requeridos" });
                }

                var sucursalCreada = await _sucursalService.CrearAsync(createDto);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = sucursalCreada.Id },
                    sucursalCreada);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Error de validacion al crear sucursal");
                return BadRequest(new { mensaje = "Errores de validacion", errores = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la sucursal");
                return StatusCode(500, new { mensaje = "Error al crear la sucursal", detalle = ex.Message });
            }
        }
    }
}
