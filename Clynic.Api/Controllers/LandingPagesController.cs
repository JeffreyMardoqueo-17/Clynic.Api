using Clynic.Application.DTOs.LandingPages;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LandingPagesController : ControllerBase
    {
        private readonly ILandingPageConfigService _landingService;

        public LandingPagesController(ILandingPageConfigService landingService)
        {
            _landingService = landingService ?? throw new ArgumentNullException(nameof(landingService));
        }

        [HttpGet("clinica/{idClinica}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LandingPageConfigResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<LandingPageConfigResponseDto>> ObtenerPorClinica(int idClinica)
        {
            if (!TieneAccesoClinica(idClinica))
            {
                return Forbid();
            }

            var config = await _landingService.ObtenerPorClinicaAsync(idClinica);
            return Ok(config);
        }

        [HttpPut("clinica/{idClinica}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(LandingPageConfigResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<LandingPageConfigResponseDto>> Guardar(int idClinica, [FromBody] UpsertLandingPageConfigDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Payload requerido." });
            }

            if (!TieneAccesoClinica(idClinica))
            {
                return Forbid();
            }

            dto.IdClinica = idClinica;
            var saved = await _landingService.GuardarAsync(dto);
            return Ok(saved);
        }

        [HttpGet("public")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LandingPublicResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LandingPublicResponseDto>> ObtenerPublica([FromQuery] string clinicaSlug)
        {
            var config = await _landingService.ObtenerPublicaAsync(clinicaSlug);
            if (config == null)
            {
                return NotFound(new { mensaje = "Landing no encontrada para la clínica indicada." });
            }

            return Ok(config);
        }

        private bool TieneAccesoClinica(int idClinica)
        {
            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            return int.TryParse(idClinicaClaim, out var idClinicaToken) && idClinicaToken == idClinica;
        }
    }
}
