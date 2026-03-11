using Clynic.Application.DTOs.CatalogoPersonal;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Application.Services
{
    public class CatalogoPersonalService : ICatalogoPersonalService
    {
        private static readonly string[] RolesBaseSucursal =
        {
            "Recepcionista",
            "Doctor"
        };

        private static readonly string[] EspecialidadesInmutablesGlobales =
        {
            "Encargado Global",
            "Atencion al Cliente"
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

                Rol rol;
                try
                {
                    rol = await _rolRepository.CrearAsync(new Rol
                    {
                        IdClinica = idClinica,
                        IdSucursal = idSucursal,
                        Nombre = nombreRol,
                        Descripcion = descripcion,
                        Activo = true
                    });
                }
                catch (DbUpdateException)
                {
                    // Si otro request creo el mismo rol en paralelo, reutilizamos el existente.
                    rol = await _rolRepository.ObtenerPorNombreEnSucursalAsync(idClinica, idSucursal, nombreRol)
                        ?? throw new InvalidOperationException("No fue posible resolver el rol creado en paralelo.");
                }

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
            if (!RolesBaseSucursal.Any(r => r.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Solo se permiten los roles Doctor y Recepcionista.");
            }

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

            // Garantiza que cada sucursal tenga siempre los roles operativos base.
            await CrearRolesBaseSucursalAsync(idClinica, idSucursal);

            var roles = await _rolRepository.ObtenerActivosPorSucursalAsync(idClinica, idSucursal);
            var resultado = roles.Select(MapRol).ToList();

            // Incluye el rol Admin global/clinica para permitir creacion de administradores desde UI.
            var rolesClinica = await _rolRepository.ObtenerActivosAsync(idClinica);
            var rolAdmin = rolesClinica.FirstOrDefault(r =>
                !r.IdSucursal.HasValue &&
                r.Nombre.Equals("Admin", StringComparison.OrdinalIgnoreCase));

            if (rolAdmin == null)
            {
                rolAdmin = await _rolRepository.CrearAsync(new Rol
                {
                    IdClinica = idClinica,
                    IdSucursal = null,
                    Nombre = "Admin",
                    Descripcion = "Administrador de la clínica",
                    Activo = true,
                });
            }

            if (!resultado.Any(r => r.Id == rolAdmin.Id))
            {
                resultado.Insert(0, MapRol(rolAdmin));
            }

            return resultado;
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
            if (EspecialidadesInmutablesGlobales.Any(e => e.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Esta especialidad es de sistema y no puede crearse ni editarse manualmente.");
            }

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

            var existente = await _sucursalEspecialidadRepository.ObtenerConfiguracionAsync(createDto.IdSucursal, createDto.IdEspecialidad);
            if (existente != null)
            {
                if (!existente.Activa)
                {
                    existente.Activa = true;
                    existente = await _sucursalEspecialidadRepository.ActualizarAsync(existente);
                }

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

        public async Task<IReadOnlyCollection<EspecialidadSucursalResponseDto>> ActualizarEstadoEspecialidadEnSucursalesAsync(ActualizarEstadoEspecialidadSucursalesDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var especialidad = await _especialidadRepository.ObtenerPorIdAsync(dto.IdEspecialidad);
            if (especialidad == null || especialidad.IdClinica != dto.IdClinica)
            {
                throw new InvalidOperationException("La especialidad no existe o no pertenece a la clínica.");
            }

            var idsValidos = dto.IdsSucursales.Where(id => id > 0).Distinct().ToList();
            if (idsValidos.Count == 0)
            {
                throw new InvalidOperationException("Debes enviar al menos una sucursal válida.");
            }

            var actualizadas = new List<EspecialidadSucursalResponseDto>();
            foreach (var idSucursal in idsValidos)
            {
                await ValidarClinicaYSucursalAsync(dto.IdClinica, idSucursal);

                var configuracion = await _sucursalEspecialidadRepository.ObtenerConfiguracionAsync(idSucursal, dto.IdEspecialidad);
                if (configuracion == null)
                {
                    if (!dto.Activa)
                    {
                        continue;
                    }

                    configuracion = await _sucursalEspecialidadRepository.CrearAsync(new SucursalEspecialidad
                    {
                        IdSucursal = idSucursal,
                        IdEspecialidad = dto.IdEspecialidad,
                        Activa = true,
                    });
                }
                else if (configuracion.Activa != dto.Activa)
                {
                    configuracion.Activa = dto.Activa;
                    configuracion = await _sucursalEspecialidadRepository.ActualizarAsync(configuracion);
                }

                actualizadas.Add(MapSucursalEspecialidad(configuracion));
            }

            return actualizadas;
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
