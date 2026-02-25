using FluentValidation;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Rules;
using Clynic.Domain.Models;
using Clynic.Domain.Models.Enums;

namespace Clynic.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IClinicaRepository _clinicaRepository;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly UsuarioRules _usuarioRules;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IClinicaRepository clinicaRepository,
            IEmailService emailService,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator,
            UsuarioRules usuarioRules)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _registerValidator = registerValidator ?? throw new ArgumentNullException(nameof(registerValidator));
            _loginValidator = loginValidator ?? throw new ArgumentNullException(nameof(loginValidator));
            _usuarioRules = usuarioRules ?? throw new ArgumentNullException(nameof(usuarioRules));
        }

        public async Task<AuthResponseDto> RegistrarAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
                throw new ArgumentNullException(nameof(registerDto));

            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            if (!await _usuarioRules.CorreoEsUnicoAsync(registerDto.Correo))
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "Ya existe un usuario con este correo electrónico"
                };
            }

            if (!await _usuarioRules.ClinicaExisteAsync(registerDto.IdClinica))
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "La clínica especificada no existe"
                };
            }

            if (registerDto.IdSucursal.HasValue &&
                !await _usuarioRules.SucursalPerteneceAClinicaAsync(registerDto.IdSucursal.Value, registerDto.IdClinica))
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "La sucursal especificada no pertenece a la clínica"
                };
            }

            var usuario = new Usuario
            {
                NombreCompleto = registerDto.NombreCompleto.Trim(),
                Correo = registerDto.Correo.Trim().ToLower(),
                ClaveHash = _passwordHasher.Hash(registerDto.Clave),
                Rol = registerDto.Rol,
                IdClinica = registerDto.IdClinica,
                IdSucursal = registerDto.IdSucursal,
                Activo = true,
                DebeCambiarClave = registerDto.Rol != UsuarioRol.Admin,
                FechaCreacion = DateTime.UtcNow
            };

            var usuarioCreado = await _usuarioRepository.CrearAsync(usuario);

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(usuarioCreado.IdClinica);
            var nombreClinica = clinica?.Nombre ?? "Clínica asignada";

            await _emailService.EnviarBienvenidaUsuarioAsync(
                usuarioCreado.Correo,
                usuarioCreado.NombreCompleto,
                nombreClinica,
                usuarioCreado.Rol.ToString());

            var token = _jwtService.GenerarToken(usuarioCreado);
            var expiracion = _jwtService.ObtenerFechaExpiracion();

            return new AuthResponseDto
            {
                Exito = true,
                Mensaje = "Usuario registrado exitosamente",
                Token = token,
                Expiracion = expiracion,
                Usuario = MapToResponseDto(usuarioCreado)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            if (loginDto == null)
                throw new ArgumentNullException(nameof(loginDto));

            var validationResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validationResult.IsValid)
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            var usuario = await _usuarioRepository
                .ObtenerPorCorreoAsync(loginDto.Correo.Trim().ToLower());

            if (usuario == null || !_passwordHasher.Verify(loginDto.Clave, usuario.ClaveHash))
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "Credenciales inválidas"
                };
            }

            if (!usuario.Activo)
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "El usuario está desactivado"
                };
            }

            var token = _jwtService.GenerarToken(usuario);
            var expiracion = _jwtService.ObtenerFechaExpiracion();

            return new AuthResponseDto
            {
                Exito = true,
                Mensaje = "Login exitoso",
                Token = token,
                Expiracion = expiracion,
                Usuario = MapToResponseDto(usuario)
            };
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
                DebeCambiarClave = usuario.DebeCambiarClave,
                IdClinica = usuario.IdClinica,
                NombreClinica = usuario.Clinica?.Nombre,
                IdSucursal = usuario.IdSucursal,
                NombreSucursal = usuario.Sucursal?.Nombre,
                FechaCreacion = usuario.FechaCreacion
            };
        }
    }
}