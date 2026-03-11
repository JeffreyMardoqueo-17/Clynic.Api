using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Clynic.Application.DTOs.Clinicas;
using Clynic.Application.Interfaces.Services;

namespace Clynic.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar las ClÃ­nicas
    /// </summary>
    [ApiController]
    [Route("/[controller]")]
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
        /// Obtiene todas las clÃ­nicas activas
        /// </summary>
        /// <returns>Lista de clÃ­nicas</returns>
        /// <response code="200">Retorna la lista de clÃ­nicas</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<ClinicaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ClinicaResponseDto>>> ObtenerTodas()
        {
            var clinicas = await _clinicaService.ObtenerTodasAsync();
            return Ok(clinicas);
        }

        /// <summary>
        /// Obtiene el listado publico de clinicas activas.
        /// </summary>
        /// <returns>Lista de clinicas activas para agendado publico</returns>
        [HttpGet("publicas")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ClinicaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ClinicaResponseDto>>> ObtenerPublicas()
        {
            var clinicas = await _clinicaService.ObtenerTodasAsync();
            return Ok(clinicas);
        }

        /// <summary>
        /// Obtiene una clÃ­nica por su ID
        /// </summary>
        /// <param name="id">ID de la clÃ­nica</param>
        /// <returns>La clÃ­nica solicitada</returns>
        /// <response code="200">Retorna la clÃ­nica encontrada</response>
        /// <response code="404">No se encontrÃ³ la clÃ­nica</response>
        /// <response code="400">ID invÃ¡lido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(ClinicaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClinicaResponseDto>> ObtenerPorId(int id)
        {

            var clinica = await _clinicaService.ObtenerPorIdAsync(id);

            if (clinica == null)
                return NotFound(new { mensaje = $"No se encontrÃ³ la clÃ­nica con ID {id}" });

            return Ok(clinica);
        }

    }
}

