using Clynic.Application.DTOs.CatalogoPersonal;
using Clynic.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class EspecialidadesDoctorController : ControllerBase
    {
        private readonly ICatalogoPersonalService _catalogoPersonalService;

        public EspecialidadesDoctorController(ICatalogoPersonalService catalogoPersonalService)
        {
            _catalogoPersonalService = catalogoPersonalService ?? throw new ArgumentNullException(nameof(catalogoPersonalService));
        }

        [HttpGet("clinica/{idClinica}")]
        [ProducesResponseType(typeof(IReadOnlyCollection<EspecialidadResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<EspecialidadResponseDto>>> ObtenerPorClinica(int idClinica)
        {
            if (!TieneAccesoClinica(idClinica))
            {
                return Forbid();
            }

            var especialidades = await _catalogoPersonalService.ObtenerEspecialidadesPorClinicaAsync(idClinica);
            return Ok(especialidades);
        }

        [HttpPost]
        [ProducesResponseType(typeof(EspecialidadResponseDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<EspecialidadResponseDto>> Crear([FromBody] CreateEspecialidadDto createDto)
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
            return CreatedAtAction(nameof(ObtenerPorClinica), new { idClinica = createDto.IdClinica }, especialidad);
        }

        [HttpGet("clinica/{idClinica}/sucursal/{idSucursal}")]
        [ProducesResponseType(typeof(IReadOnlyCollection<EspecialidadSucursalResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<EspecialidadSucursalResponseDto>>> ObtenerPorSucursal(int idClinica, int idSucursal)
        {
            if (!TieneAccesoClinica(idClinica))
            {
                return Forbid();
            }

            var especialidades = await _catalogoPersonalService.ObtenerEspecialidadesPorSucursalAsync(idClinica, idSucursal);
            return Ok(especialidades);
        }

        [HttpPost("asignar-sucursales")]
        [ProducesResponseType(typeof(IReadOnlyCollection<EspecialidadSucursalResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<EspecialidadSucursalResponseDto>>> AsignarASucursales([FromBody] AsignarEspecialidadDoctorSucursalesDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos de asignacion son requeridos" });
            }

            if (!TieneAccesoClinica(dto.IdClinica))
            {
                return Forbid();
            }

            var idsSucursalValidos = dto.IdsSucursales
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (idsSucursalValidos.Count == 0)
            {
                return BadRequest(new { mensaje = "Debes enviar al menos una sucursal valida." });
            }

            var asignadas = new List<EspecialidadSucursalResponseDto>();
            foreach (var idSucursal in idsSucursalValidos)
            {
                var asignada = await _catalogoPersonalService.AsignarEspecialidadASucursalAsync(new AsignarEspecialidadSucursalDto
                {
                    IdClinica = dto.IdClinica,
                    IdSucursal = idSucursal,
                    IdEspecialidad = dto.IdEspecialidad,
                });

                asignadas.Add(asignada);
            }

            return Ok(asignadas);
        }

        [HttpPatch("estado-sucursales")]
        [ProducesResponseType(typeof(IReadOnlyCollection<EspecialidadSucursalResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyCollection<EspecialidadSucursalResponseDto>>> ActualizarEstadoEnSucursales(
            [FromBody] ActualizarEstadoEspecialidadSucursalesDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensaje = "Los datos de actualización son requeridos" });
            }

            if (!TieneAccesoClinica(dto.IdClinica))
            {
                return Forbid();
            }

            var actualizadas = await _catalogoPersonalService.ActualizarEstadoEspecialidadEnSucursalesAsync(dto);
            return Ok(actualizadas);
        }

        private bool TieneAccesoClinica(int idClinica)
        {
            var idClinicaClaim = User.FindFirst("IdClinica")?.Value;
            return int.TryParse(idClinicaClaim, out var idClinicaToken) && idClinicaToken == idClinica;
        }
    }
}
