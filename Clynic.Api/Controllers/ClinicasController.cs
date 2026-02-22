using Microsoft.AspNetCore.Mvc;
using Clynic.Application.DTOs.Clinicas;
using Clynic.Application.Interfaces.Services;

namespace Clynic.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar las Clínicas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClinicasController : ControllerBase
    {
        private readonly IClinicaService _clinicaService;

        public ClinicasController(
            IClinicaService clinicaService)
        {
            _clinicaService = clinicaService ?? throw new ArgumentNullException(nameof(clinicaService));
        }

        /// <summary>
        /// Obtiene todas las clínicas activas
        /// </summary>
        /// <returns>Lista de clínicas</returns>
        /// <response code="200">Retorna la lista de clínicas</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClinicaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ClinicaResponseDto>>> ObtenerTodas()
        {
            var clinicas = await _clinicaService.ObtenerTodasAsync();
            return Ok(clinicas);
        }

        /// <summary>
        /// Obtiene una clínica por su ID
        /// </summary>
        /// <param name="id">ID de la clínica</param>
        /// <returns>La clínica solicitada</returns>
        /// <response code="200">Retorna la clínica encontrada</response>
        /// <response code="404">No se encontró la clínica</response>
        /// <response code="400">ID inválido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClinicaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClinicaResponseDto>> ObtenerPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
            }

            var clinica = await _clinicaService.ObtenerPorIdAsync(id);

            if (clinica == null)
            {
                return NotFound(new { mensaje = $"No se encontró la clínica con ID {id}" });
            }

            return Ok(clinica);
        }

        /// <summary>
        /// Crea una nueva clínica
        /// </summary>
        /// <param name="createDto">Datos de la nueva clínica</param>
        /// <returns>La clínica creada</returns>
        /// <response code="201">Clínica creada exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(ClinicaResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClinicaResponseDto>> Crear([FromBody] CreateClinicaDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { mensaje = "Los datos de la clínica son requeridos" });
            }

            var clinicaCreada = await _clinicaService.CrearAsync(createDto);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = clinicaCreada.Id },
                clinicaCreada);
        }
    }
}
