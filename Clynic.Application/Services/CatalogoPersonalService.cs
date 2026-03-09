using Clynic.Application.DTOs.CatalogoPersonal;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;

namespace Clynic.Application.Services
{
    public class CatalogoPersonalService : ICatalogoPersonalService
    {
        private static readonly string[] RolesBaseSucursal =
        {
            "Recepcionista",
            "Doctor"
        };

        private readonly IClinicaRepository _clinicaRepository;
        private readonly ISucursalRepository _sucursalRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IEspecialidadRepository _especialidadRepository;
        private readonly ISucursalEspecialidadRepository _sucursalEspecialidadRepository;

        public CatalogoPersonalService(
            IClinicaRepository clinicaRepository,
            ISucursalRepository sucursalRepository,
            IRolRepository rolRepository,
            IEspecialidadRepository especialidadRepository,
            ISucursalEspecialidadRepository sucursalEspecialidadRepository)
        {
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
            _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
            _especialidadRepository = especialidadRepository ?? throw new ArgumentNullException(nameof(especialidadRepository));
            _sucursalEspecialidadRepository = sucursalEspecialidadRepository ?? throw new ArgumentNullException(nameof(sucursalEspecialidadRepository));
        }

        public async Task<IReadOnlyCollection<RolSucursalResponseDto>> CrearRolesBaseSucursalAsync(int idClinica, int idSucursal)
        {
            await ValidarClinicaYSucursalAsync(idClinica, idSucursal);

            var creados = new List<RolSucursalResponseDto>();
            foreach (var nombreRol in RolesBaseSucursal)
            {
                var existente = await _rolRepository.ObtenerPorNombreEnSucursalAsync(idClinica, idSucursal, nombreRol);
                if (existente != null)
                {
                    creados.Add(MapRol(existente));
                    continue;
                }

                var descripcion = nombreRol.Equals("Recepcionista", StringComparison.OrdinalIgnoreCase)
                    ? "Encargado de recibir a las personas y mantener el orden de atencion en la sucursal"
                    : "Profesional de salud que atiende consultas y requiere especialidad";

                var rol = await _rolRepository.CrearAsync(new Rol
                {
                    IdClinica = idClinica,
                    IdSucursal = idSucursal,
                    Nombre = nombreRol,
                    Descripcion = descripcion,
                    Activo = true
                });

                creados.Add(MapRol(rol));
            }

            return creados;
        }

        public async Task<RolSucursalResponseDto> CrearRolAsync(CreateRolSucursalDto createDto)
        {
            if (createDto == null)
            {
                throw new ArgumentNullException(nameof(createDto));
            }

            await ValidarClinicaYSucursalAsync(createDto.IdClinica, createDto.IdSucursal);

            var nombre = createDto.Nombre.Trim();
            var existente = await _rolRepository.ObtenerPorNombreEnSucursalAsync(createDto.IdClinica, createDto.IdSucursal, nombre);
            if (existente != null)
            {
                throw new InvalidOperationException("Ya existe un rol con ese nombre en la sucursal.");
            }

            var rol = await _rolRepository.CrearAsync(new Rol
            {
                IdClinica = createDto.IdClinica,
                IdSucursal = createDto.IdSucursal,
                Nombre = nombre,
                Descripcion = createDto.Descripcion?.Trim() ?? string.Empty,
                Activo = true
            });

            return MapRol(rol);
        }

        public async Task<IReadOnlyCollection<RolSucursalResponseDto>> ObtenerRolesPorSucursalAsync(int idClinica, int idSucursal)
        {
            await ValidarClinicaYSucursalAsync(idClinica, idSucursal);

            var roles = await _rolRepository.ObtenerActivosPorSucursalAsync(idClinica, idSucursal);
            return roles.Select(MapRol).ToList();
        }

        public async Task<EspecialidadResponseDto> CrearEspecialidadAsync(CreateEspecialidadDto createDto)
        {
            if (createDto == null)
            {
                throw new ArgumentNullException(nameof(createDto));
            }

            var clinicaExiste = await _clinicaRepository.ExisteAsync(createDto.IdClinica);
            if (!clinicaExiste)
            {
                throw new KeyNotFoundException("La clinica especificada no existe o esta inactiva.");
            }

            var nombre = createDto.Nombre.Trim();
            var existente = await _especialidadRepository.ObtenerPorNombreAsync(createDto.IdClinica, nombre);
            if (existente != null)
            {
                throw new InvalidOperationException("Ya existe una especialidad con ese nombre en la clinica.");
            }

            var especialidad = await _especialidadRepository.CrearAsync(new Especialidad
            {
                IdClinica = createDto.IdClinica,
                Nombre = nombre,
                Descripcion = createDto.Descripcion?.Trim() ?? string.Empty,
                Activa = true
            });

            return MapEspecialidad(especialidad);
        }

        public async Task<IReadOnlyCollection<EspecialidadResponseDto>> ObtenerEspecialidadesPorClinicaAsync(int idClinica)
        {
            var clinicaExiste = await _clinicaRepository.ExisteAsync(idClinica);
            if (!clinicaExiste)
            {
                throw new KeyNotFoundException("La clinica especificada no existe o esta inactiva.");
            }

            var especialidades = await _especialidadRepository.ObtenerActivasAsync(idClinica);
            return especialidades.Select(MapEspecialidad).ToList();
        }

        public async Task<EspecialidadSucursalResponseDto> AsignarEspecialidadASucursalAsync(AsignarEspecialidadSucursalDto createDto)
        {
            if (createDto == null)
            {
                throw new ArgumentNullException(nameof(createDto));
            }

            await ValidarClinicaYSucursalAsync(createDto.IdClinica, createDto.IdSucursal);

            var especialidad = await _especialidadRepository.ObtenerPorIdAsync(createDto.IdEspecialidad);
            if (especialidad == null || !especialidad.Activa || especialidad.IdClinica != createDto.IdClinica)
            {
                throw new InvalidOperationException("La especialidad no existe, esta inactiva o no pertenece a la clinica.");
            }

            var existente = await _sucursalEspecialidadRepository.ObtenerConfiguracionActivaAsync(createDto.IdSucursal, createDto.IdEspecialidad);
            if (existente != null)
            {
                return MapSucursalEspecialidad(existente);
            }

            var creada = await _sucursalEspecialidadRepository.CrearAsync(new SucursalEspecialidad
            {
                IdSucursal = createDto.IdSucursal,
                IdEspecialidad = createDto.IdEspecialidad,
                Activa = true
            });

            return MapSucursalEspecialidad(creada);
        }

        public async Task<IReadOnlyCollection<EspecialidadSucursalResponseDto>> ObtenerEspecialidadesPorSucursalAsync(int idClinica, int idSucursal)
        {
            await ValidarClinicaYSucursalAsync(idClinica, idSucursal);

            var especialidades = await _sucursalEspecialidadRepository.ObtenerActivasPorSucursalAsync(idSucursal);
            return especialidades.Select(MapSucursalEspecialidad).ToList();
        }

        private async Task ValidarClinicaYSucursalAsync(int idClinica, int idSucursal)
        {
            var clinicaExiste = await _clinicaRepository.ExisteAsync(idClinica);
            if (!clinicaExiste)
            {
                throw new KeyNotFoundException("La clinica especificada no existe o esta inactiva.");
            }

            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(idSucursal);
            if (sucursal == null || !sucursal.Activa || sucursal.IdClinica != idClinica)
            {
                throw new KeyNotFoundException("La sucursal no existe, esta inactiva o no pertenece a la clinica.");
            }
        }

        private static RolSucursalResponseDto MapRol(Rol rol)
        {
            return new RolSucursalResponseDto
            {
                Id = rol.Id,
                IdClinica = rol.IdClinica,
                IdSucursal = rol.IdSucursal,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion,
                Activo = rol.Activo
            };
        }

        private static EspecialidadResponseDto MapEspecialidad(Especialidad especialidad)
        {
            return new EspecialidadResponseDto
            {
                Id = especialidad.Id,
                IdClinica = especialidad.IdClinica,
                Nombre = especialidad.Nombre,
                Descripcion = especialidad.Descripcion,
                Activa = especialidad.Activa
            };
        }

        private static EspecialidadSucursalResponseDto MapSucursalEspecialidad(SucursalEspecialidad item)
        {
            return new EspecialidadSucursalResponseDto
            {
                Id = item.Id,
                IdSucursal = item.IdSucursal,
                IdEspecialidad = item.IdEspecialidad,
                NombreEspecialidad = item.Especialidad?.Nombre ?? string.Empty,
                Activa = item.Activa
            };
        }
    }
}
