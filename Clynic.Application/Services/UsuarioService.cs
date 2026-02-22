using FluentValidation;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Rules;
using Clynic.Domain.Models;

namespace Clynic.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly IClinicaRepository _clinicaRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<RegisterDto> _createValidator;
        private readonly IValidator<UpdateUsuarioDto> _updateValidator;
        private readonly UsuarioRules _rules;

        public UsuarioService(
            IUsuarioRepository repository,
            IClinicaRepository clinicaRepository,
            IPasswordHasher passwordHasher,
            IValidator<RegisterDto> createValidator,
            IValidator<UpdateUsuarioDto> updateValidator,
            UsuarioRules rules)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public async Task<IEnumerable<UsuarioResponseDto>> ObtenerTodosAsync()
        {
            var usuarios = await _repository.ObtenerTodosAsync();
            return usuarios.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<UsuarioResponseDto>> ObtenerPorClinicaAsync(int idClinica)
        {
            var usuarios = await _repository.ObtenerPorClinicaAsync(idClinica);
            return usuarios.Select(MapToResponseDto);
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

            var usuario = new Usuario
            {
                NombreCompleto = createDto.NombreCompleto.Trim(),
                Correo = createDto.Correo.Trim().ToLower(),
                ClaveHash = _passwordHasher.Hash(createDto.Clave),
                Rol = createDto.Rol,
                IdClinica = createDto.IdClinica,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            var usuarioCreado = await _repository.CrearAsync(usuario);
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

            if (updateDto.Rol.HasValue)
                usuario.Rol = updateDto.Rol.Value;

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
                Rol = usuario.Rol,
                Activo = usuario.Activo,
                IdClinica = usuario.IdClinica,
                NombreClinica = usuario.Clinica?.Nombre,
                FechaCreacion = usuario.FechaCreacion
            };
        }
    }
}
