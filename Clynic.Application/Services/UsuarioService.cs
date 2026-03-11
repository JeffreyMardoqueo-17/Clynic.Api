using FluentValidation;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Rules;
using Clynic.Domain.Models;
using System.Security.Cryptography;

namespace Clynic.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly IClinicaRepository _clinicaRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IEspecialidadRepository _especialidadRepository;
        private readonly ISucursalEspecialidadRepository _sucursalEspecialidadRepository;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<RegisterDto> _createValidator;
        private readonly IValidator<CreateUsuarioAdminDto> _createAdminValidator;
        private readonly IValidator<UpdateUsuarioDto> _updateValidator;
        private readonly UsuarioRules _rules;

        public UsuarioService(
            IUsuarioRepository repository,
            IClinicaRepository clinicaRepository,
            IRolRepository rolRepository,
            IEspecialidadRepository especialidadRepository,
            ISucursalEspecialidadRepository sucursalEspecialidadRepository,
            IEmailService emailService,
            IPasswordHasher passwordHasher,
            IValidator<RegisterDto> createValidator,
            IValidator<CreateUsuarioAdminDto> createAdminValidator,
            IValidator<UpdateUsuarioDto> updateValidator,
            UsuarioRules rules)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
            _especialidadRepository = especialidadRepository ?? throw new ArgumentNullException(nameof(especialidadRepository));
            _sucursalEspecialidadRepository = sucursalEspecialidadRepository ?? throw new ArgumentNullException(nameof(sucursalEspecialidadRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _createAdminValidator = createAdminValidator ?? throw new ArgumentNullException(nameof(createAdminValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public async Task<IEnumerable<UsuarioResponseDto>> ObtenerTodosAsync()
        {
            var usuarios = await _repository.ObtenerTodosAsync();
            return usuarios.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<UsuarioResponseDto>> ObtenerPorClinicaAsync(int idClinica, string? busquedaNombre = null)
        {
            var usuarios = await _repository.ObtenerPorClinicaAsync(idClinica, busquedaNombre);
            return usuarios.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<UsuarioResponseDto>> ObtenerPorClinicaYSucursalAsync(int idClinica, int idSucursal, string? busquedaNombre = null)
        {
            var usuarios = await _repository.ObtenerPorClinicaYSucursalAsync(idClinica, idSucursal, busquedaNombre);
            return usuarios.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<UsuarioResponseDto>> ObtenerInactivosPorClinicaAsync(int idClinica, int? idSucursal = null, string? busquedaNombre = null)
        {
            var usuarios = await _repository.ObtenerInactivosPorClinicaAsync(idClinica, idSucursal, busquedaNombre);
            return usuarios.Select(MapToResponseDto);
        }

        public async Task<UsuarioResponseDto> ObtenerPerfilAsync(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID del usuario autenticado debe ser mayor a cero.", nameof(idUsuario));

            var usuario = await _repository.ObtenerPorIdAsync(idUsuario);
            if (usuario == null)
                throw new KeyNotFoundException("Usuario no encontrado");

            if (!usuario.Activo)
                throw new UnauthorizedAccessException("El usuario está inactivo");

            return MapToResponseDto(usuario);
        }

        public async Task<UsuarioResponseDto?> ObtenerPorIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            var usuario = await _repository.ObtenerPorIdAsync(id);
            return usuario != null ? MapToResponseDto(usuario) : null;
        }

        public async Task<UsuarioResponseDto?> ObtenerPorCorreoAsync(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return null;

            var usuario = await _repository.ObtenerPorCorreoAsync(correo);
            return usuario != null ? MapToResponseDto(usuario) : null;
        }

        public async Task<UsuarioResponseDto> CrearAsync(RegisterDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            if (!await _rules.CorreoEsUnicoAsync(createDto.Correo))
                throw new InvalidOperationException("Ya existe un usuario con este correo electrónico");

            if (!await _rules.ClinicaExisteAsync(createDto.IdClinica))
                throw new InvalidOperationException("La clínica especificada no existe");

            if (createDto.IdSucursal.HasValue &&
                !await _rules.SucursalPerteneceAClinicaAsync(createDto.IdSucursal.Value, createDto.IdClinica))
            {
                throw new InvalidOperationException("La sucursal especificada no pertenece a la clínica");
            }

            var rol = await ValidarConfiguracionRolAsync(
                createDto.IdRol,
                createDto.IdClinica,
                createDto.IdSucursal,
                createDto.IdEspecialidad);

            var nombreRol = rol.Nombre?.Trim() ?? string.Empty;
            var esAdmin = EsRolAdmin(nombreRol);

            var usuario = new Usuario
            {
                NombreCompleto = createDto.NombreCompleto.Trim(),
                Correo = createDto.Correo.Trim().ToLower(),
                ClaveHash = _passwordHasher.Hash(createDto.Clave),
                IdRol = createDto.IdRol,
                IdEspecialidad = RolRequiereEspecialidad(nombreRol) ? createDto.IdEspecialidad : null,
                IdClinica = createDto.IdClinica,
                IdSucursal = createDto.IdSucursal,
                Activo = true,
                DebeCambiarClave = !esAdmin,
                FechaCreacion = DateTime.UtcNow
            };

            var usuarioCreado = await _repository.CrearAsync(usuario);

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(usuarioCreado.IdClinica);
            var nombreClinica = clinica?.Nombre ?? "Clínica asignada";

            await _emailService.EnviarBienvenidaUsuarioAsync(
                usuarioCreado.Correo,
                usuarioCreado.NombreCompleto,
                nombreClinica,
                nombreRol);

            return MapToResponseDto(usuarioCreado);
        }

        public async Task<UsuarioResponseDto> CrearPorAdminAsync(CreateUsuarioAdminDto createDto)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var validationResult = await _createAdminValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var claveTemporal = GenerarClaveTemporal(12);

            var rol = await ValidarConfiguracionRolAsync(
                createDto.IdRol,
                createDto.IdClinica,
                createDto.IdSucursal,
                createDto.IdEspecialidad);

            var nombreRolCreate = rol.Nombre?.Trim() ?? string.Empty;
            var esAdmin = EsRolAdmin(nombreRolCreate);

            var usuario = new Usuario
            {
                NombreCompleto = createDto.NombreCompleto.Trim(),
                Correo = createDto.Correo.Trim().ToLower(),
                ClaveHash = _passwordHasher.Hash(claveTemporal),
                IdRol = createDto.IdRol,
                IdEspecialidad = RolRequiereEspecialidad(nombreRolCreate) ? createDto.IdEspecialidad : null,
                IdClinica = createDto.IdClinica,
                IdSucursal = createDto.IdSucursal,
                Activo = true,
                DebeCambiarClave = !esAdmin,
                FechaCreacion = DateTime.UtcNow
            };

            var usuarioCreado = await _repository.CrearAsync(usuario);

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(usuarioCreado.IdClinica);
            var nombreClinica = clinica?.Nombre ?? "Clínica asignada";

            await _emailService.EnviarCredencialesTemporalesAsync(
                usuarioCreado.Correo,
                usuarioCreado.NombreCompleto,
                nombreClinica,
                nombreRolCreate,
                claveTemporal);

            return MapToResponseDto(usuarioCreado);
        }

        public async Task<UsuarioResponseDto?> ActualizarAsync(int id, UpdateUsuarioDto updateDto)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));

            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Errores de validación: {errores}");
            }

            var usuario = await _repository.ObtenerPorIdAsync(id);
            if (usuario == null)
                return null;

            if (!string.IsNullOrWhiteSpace(updateDto.NombreCompleto))
                usuario.NombreCompleto = updateDto.NombreCompleto.Trim();

            if (!string.IsNullOrWhiteSpace(updateDto.Correo) && updateDto.Correo.ToLower() != usuario.Correo)
            {
                if (!await _rules.CorreoEsUnicoAsync(updateDto.Correo, id))
                    throw new InvalidOperationException("Ya existe otro usuario con este correo electrónico");
                
                usuario.Correo = updateDto.Correo.Trim().ToLower();
            }

            var rolObjetivoId = updateDto.IdRol ?? usuario.IdRol;
            var sucursalObjetivoId = updateDto.IdSucursal ?? usuario.IdSucursal;
            var especialidadObjetivoId = updateDto.IdEspecialidad ?? usuario.IdEspecialidad;

            var rolObjetivo = await _rolRepository.ObtenerPorIdAsync(rolObjetivoId)
                ?? throw new InvalidOperationException("El rol especificado no existe.");

            var nombreRolObjetivo = rolObjetivo.Nombre?.Trim() ?? string.Empty;
            await ValidarConfiguracionRolAsync(
                rolObjetivoId,
                usuario.IdClinica,
                sucursalObjetivoId,
                especialidadObjetivoId);

            if (!RolRequiereEspecialidad(nombreRolObjetivo))
            {
                especialidadObjetivoId = null;
            }

            usuario.IdRol = rolObjetivoId;
            usuario.IdSucursal = sucursalObjetivoId;
            usuario.IdEspecialidad = especialidadObjetivoId;

            if (updateDto.Activo.HasValue)
                usuario.Activo = updateDto.Activo.Value;

            var usuarioActualizado = await _repository.ActualizarAsync(usuario);
            return MapToResponseDto(usuarioActualizado);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            return await _repository.EliminarAsync(id);
        }

        public async Task<bool> ReenviarCredencialesTemporalesAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            var usuario = await _repository.ObtenerPorIdAsync(id);
            if (usuario == null)
                return false;

            var claveTemporal = GenerarClaveTemporal(12);
            usuario.ClaveHash = _passwordHasher.Hash(claveTemporal);
            usuario.DebeCambiarClave = true;

            await _repository.ActualizarAsync(usuario);

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(usuario.IdClinica);
            var nombreClinica = clinica?.Nombre ?? "Clínica asignada";

            await _emailService.EnviarCredencialesTemporalesAsync(
                usuario.Correo,
                usuario.NombreCompleto,
                nombreClinica,
                string.IsNullOrWhiteSpace(usuario.Rol?.Nombre) ? "Usuario" : usuario.Rol!.Nombre,
                claveTemporal);

            return true;
        }

        public async Task<bool> CambiarClaveAsync(int id, ChangePasswordDto changePasswordDto)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            if (changePasswordDto == null)
                throw new ArgumentNullException(nameof(changePasswordDto));

            var usuario = await _repository.ObtenerPorIdAsync(id);
            if (usuario == null)
                return false;

            if (!_passwordHasher.Verify(changePasswordDto.ClaveActual, usuario.ClaveHash))
                throw new UnauthorizedAccessException("La clave actual es incorrecta");

            usuario.ClaveHash = _passwordHasher.Hash(changePasswordDto.NuevaClave);
            usuario.DebeCambiarClave = false;
            await _repository.ActualizarAsync(usuario);

            return true;
        }

        public async Task<bool> ActualizarClaveAsync(int id, string nuevaClaveHash)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));

            if (string.IsNullOrWhiteSpace(nuevaClaveHash))
                throw new ArgumentException("La nueva clave es requerida", nameof(nuevaClaveHash));

            var usuario = await _repository.ObtenerPorIdAsync(id);
            if (usuario == null)
                return false;

            usuario.ClaveHash = nuevaClaveHash;
            usuario.DebeCambiarClave = false;
            await _repository.ActualizarAsync(usuario);

            return true;
        }

        private static UsuarioResponseDto MapToResponseDto(Usuario usuario)
        {
            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Correo,
                IdRol = usuario.IdRol,
                NombreRol = usuario.Rol?.Nombre ?? string.Empty,
                DescripcionRol = usuario.Rol?.Descripcion ?? string.Empty,
                IdEspecialidad = usuario.IdEspecialidad,
                NombreEspecialidad = usuario.Especialidad?.Nombre,
                Activo = usuario.Activo,
                DebeCambiarClave = usuario.DebeCambiarClave,
                IdClinica = usuario.IdClinica,
                NombreClinica = usuario.Clinica?.Nombre,
                IdSucursal = usuario.IdSucursal,
                NombreSucursal = usuario.Sucursal?.Nombre,
                FechaCreacion = usuario.FechaCreacion
            };
        }

        private async Task<Rol> ValidarConfiguracionRolAsync(int idRol, int idClinica, int? idSucursal, int? idEspecialidad)
        {
            var rol = await _rolRepository.ObtenerPorIdAsync(idRol)
                ?? throw new InvalidOperationException("El rol especificado no existe.");

            if (!rol.Activo)
            {
                throw new InvalidOperationException("El rol especificado está inactivo.");
            }

            var nombreRol = rol.Nombre?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(nombreRol))
            {
                throw new InvalidOperationException("El rol especificado no tiene un nombre válido.");
            }

            if (!EsRolPermitido(nombreRol))
            {
                throw new InvalidOperationException("Solo se permiten los roles Admin, Doctor y Recepcionista.");
            }

            if (rol.IdClinica.HasValue && rol.IdClinica.Value != idClinica)
            {
                throw new InvalidOperationException("El rol especificado no pertenece a la clínica del usuario.");
            }

            if (rol.IdSucursal.HasValue)
            {
                if (!idSucursal.HasValue)
                {
                    throw new InvalidOperationException("El rol especificado requiere una sucursal asignada.");
                }

                if (rol.IdSucursal.Value != idSucursal.Value)
                {
                    throw new InvalidOperationException("El rol especificado no pertenece a la sucursal indicada.");
                }
            }

            if (idSucursal.HasValue && !await _rules.SucursalPerteneceAClinicaAsync(idSucursal.Value, idClinica))
            {
                throw new InvalidOperationException("La sucursal especificada no pertenece a la clínica.");
            }

            if (RolRequiereEspecialidad(nombreRol))
            {
                if (!idSucursal.HasValue)
                {
                    throw new InvalidOperationException("El rol Doctor requiere una sucursal asignada.");
                }

                if (!idEspecialidad.HasValue)
                {
                    throw new InvalidOperationException("El rol Doctor requiere una especialidad activa para la sucursal.");
                }

                var especialidadExiste = await _especialidadRepository.ExisteActivaAsync(idClinica, idEspecialidad.Value);
                if (!especialidadExiste)
                {
                    throw new InvalidOperationException("La especialidad especificada no existe o no está activa para la clínica.");
                }

                var especialidadSucursal = await _sucursalEspecialidadRepository.ObtenerConfiguracionActivaAsync(
                    idSucursal.Value,
                    idEspecialidad.Value);

                if (especialidadSucursal == null)
                {
                    throw new InvalidOperationException("La especialidad especificada no está activa para la sucursal.");
                }

                return rol;
            }

            if (idEspecialidad.HasValue)
            {
                throw new InvalidOperationException("Solo el rol Doctor puede tener especialidad asignada.");
            }

            return rol;
        }

        private static bool EsRolAdmin(string nombreRol)
        {
            return nombreRol.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsRolRecepcionista(string nombreRol)
        {
            return nombreRol.Equals("Recepcionista", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsRolPermitido(string nombreRol)
        {
            return EsRolAdmin(nombreRol) || EsRolRecepcionista(nombreRol) || RolRequiereEspecialidad(nombreRol);
        }

        private static bool RolRequiereEspecialidad(string nombreRol)
        {
            return nombreRol.Equals("Doctor", StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerarClaveTemporal(int longitud)
        {
            const string mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string minusculas = "abcdefghijkmnopqrstuvwxyz";
            const string numeros = "23456789";
            const string simbolos = "!@#$%*?";

            var todos = mayusculas + minusculas + numeros + simbolos;
            var clave = new List<char>(longitud)
            {
                mayusculas[RandomNumberGenerator.GetInt32(mayusculas.Length)],
                minusculas[RandomNumberGenerator.GetInt32(minusculas.Length)],
                numeros[RandomNumberGenerator.GetInt32(numeros.Length)],
                simbolos[RandomNumberGenerator.GetInt32(simbolos.Length)]
            };

            for (var i = clave.Count; i < longitud; i++)
            {
                clave.Add(todos[RandomNumberGenerator.GetInt32(todos.Length)]);
            }

            for (var i = clave.Count - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (clave[i], clave[j]) = (clave[j], clave[i]);
            }

            return new string(clave.ToArray());
        }
    }
}
