using Clynic.Application.DTOs.CatalogoPersonal;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class CatalogoPersonalController : ControllerBase
    {
        private readonly ICatalogoPersonalService _catalogoPersonalService;

        public CatalogoPersonalController(ICatalogoPersonalService catalogoPersonalService)
        {
            _catalogoPersonalService = catalogoPersonalService ?? throw new ArgumentNullException(nameof(catalogoPersonalService));
        }

        [HttpPost("roles/plantilla")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IReadOnlyCollection<RolSucursalResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<RolSucursalResponseDto>>> CrearRolesBase(
            [FromQuery] int idClinica,
            [FromQuery] int idSucursal)
        {
            if (!TieneAccesoClinica(idClinica))
            {
                return Forbid();
            }

            var roles = await _catalogoPersonalService.CrearRolesBaseSucursalAsync(idClinica, idSucursal);
            return Ok(roles);
        }

        [HttpPost("roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RolSucursalResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<RolSucursalResponseDto>> CrearRol([FromBody] CreateRolSucursalDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { mensaje = "Los datos del rol son requeridos" });
            }

            if (!TieneAccesoClinica(createDto.IdClinica))
            {
                return Forbid();
            }

            var rolCreado = await _catalogoPersonalService.CrearRolAsync(createDto);
            return CreatedAtAction(nameof(ObtenerRolesPorSucursal), new { idClinica = createDto.IdClinica, idSucursal = createDto.IdSucursal }, rolCreado);
        }

        [HttpGet("roles/clinica/{idClinica}/sucursal/{idSucursal}")]
        [Authorize(Roles = "Admin,Recepcionista")]
        [ProducesResponseType(typeof(IReadOnlyCollection<RolSucursalResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<RolSucursalResponseDto>>> ObtenerRolesPorSucursal(int idClinica, int idSucursal)
        {
            if (!TieneAccesoClinica(idClinica))
            {
                return Forbid();
            }

            var roles = await _catalogoPersonalService.ObtenerRolesPorSucursalAsync(idClinica, idSucursal);
            return Ok(roles);
        }

        [HttpPost("especialidades")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EspecialidadResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<EspecialidadResponseDto>> CrearEspecialidad([FromBody] CreateEspecialidadDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { mensaje = "Los datos de la especialidad son requeridos" });
            }

            if (!TieneAccesoClinica(createDto.IdClinica))
            {
                return Forbid();
            }

            var especialidad = await _catalogoPersonalService.CrearEspecialidadAsync(createDto);
            return CreatedAtAction(nameof(ObtenerEspecialidadesPorClinica), new { idClinica = createDto.IdClinica }, especialidad);
        }

        [HttpGet("especialidades/clinica/{idClinica}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IReadOnlyCollection<EspecialidadResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<EspecialidadResponseDto>>> ObtenerEspecialidadesPorClinica(int idClinica)
        {
            if (!TieneAccesoClinica(idClinica))
            {
                return Forbid();
            }

            var especialidades = await _catalogoPersonalService.ObtenerEspecialidadesPorClinicaAsync(idClinica);
            return Ok(especialidades);
        }

        [HttpPost("especialidades/sucursal")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(EspecialidadSucursalResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<EspecialidadSucursalResponseDto>> AsignarEspecialidadASucursal([FromBody] AsignarEspecialidadSucursalDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest(new { mensaje = "Los datos de asignacion son requeridos" });
            }

            if (!TieneAccesoClinica(createDto.IdClinica))
            {
                return Forbid();
            }

            var asignacion = await _catalogoPersonalService.AsignarEspecialidadASucursalAsync(createDto);
            return CreatedAtAction(nameof(ObtenerEspecialidadesPorSucursal), new { idClinica = createDto.IdClinica, idSucursal = createDto.IdSucursal }, asignacion);
        }

        [HttpGet("especialidades/clinica/{idClinica}/sucursal/{idSucursal}")]
        [Authorize(Roles = "Admin,Doctor,Recepcionista")]
        [ProducesResponseType(typeof(IReadOnlyCollection<EspecialidadSucursalResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<EspecialidadSucursalResponseDto>>> ObtenerEspecialidadesPorSucursal(int idClinica, int idSucursal)
        {
            if (!TieneAccesoClinica(idClinica))
            {
                return Forbid();
            }

            var especialidades = await _catalogoPersonalService.ObtenerEspecialidadesPorSucursalAsync(idClinica, idSucursal);
            return Ok(especialidades);
        }

        private bool TieneAccesoClinica(int idClinica)
        {
            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            return int.TryParse(idClinicaClaim, out var idClinicaToken) && idClinicaToken == idClinica;
        }
    }
}
