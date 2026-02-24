using Microsoft.AspNetCore.Mvc;
using Clynic.Application.DTOs.Sucursales;
using Clynic.Application.Interfaces.Services;

namespace Clynic.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar las Sucursales
    /// </summary>
    [ApiController]
    [Route("/[controller]")]
    [Produces("application/json")]
    public class SucursalesController : ControllerBase
    {
        private readonly ISucursalService _sucursalService;

        public SucursalesController(
            ISucursalService sucursalService)
        {
            _sucursalService = sucursalService ?? throw new ArgumentNullException(nameof(sucursalService));
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
            var sucursales = await _sucursalService.ObtenerTodasAsync();
            return Ok(sucursales);
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
    }
}
