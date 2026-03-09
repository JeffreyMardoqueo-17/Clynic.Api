using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Clynic.Application.DTOs.Clinicas;
using Clynic.Application.Interfaces.Services;

namespace Clynic.Api.Controllers
{
    /// <summary>
    /// Controller para gestionar las Clínicas
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
        /// Obtiene todas las clínicas activas
        /// </summary>
        /// <returns>Lista de clínicas</returns>
        /// <response code="200">Retorna la lista de clínicas</response>
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
        /// Obtiene una clínica por su ID
        /// </summary>
        /// <param name="id">ID de la clínica</param>
        /// <returns>La clínica solicitada</returns>
        /// <response code="200">Retorna la clínica encontrada</response>
        /// <response code="404">No se encontró la clínica</response>
        /// <response code="400">ID inválido</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Nutricionista,Fisioterapeuta,Recepcionista")]
        [ProducesResponseType(typeof(ClinicaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ClinicaResponseDto>> ObtenerPorId(int id)
        {

            var clinica = await _clinicaService.ObtenerPorIdAsync(id);

            if (clinica == null)
                return NotFound(new { mensaje = $"No se encontró la clínica con ID {id}" });

            return Ok(clinica);
        }

    }
}
