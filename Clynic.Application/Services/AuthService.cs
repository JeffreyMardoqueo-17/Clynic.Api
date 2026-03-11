using FluentValidation;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Interfaces.Repositories;
using Clynic.Application.Interfaces.Services;
using Clynic.Application.Rules;
using Clynic.Domain.Models;
using System.Data;

namespace Clynic.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string RolAdminNombre = "Admin";
        private const string EspecialidadAdminNombre = "Encargado Global";
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IClinicaRepository _clinicaRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IRolEspecialidadRepository _rolEspecialidadRepository;
        private readonly IEspecialidadRepository _especialidadRepository;
        private readonly ISucursalEspecialidadRepository _sucursalEspecialidadRepository;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<RegisterClinicDto> _registerClinicValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly UsuarioRules _usuarioRules;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IClinicaRepository clinicaRepository,
            IRolRepository rolRepository,
            IRolEspecialidadRepository rolEspecialidadRepository,
            IEspecialidadRepository especialidadRepository,
            ISucursalEspecialidadRepository sucursalEspecialidadRepository,
            IEmailService emailService,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            IValidator<RegisterDto> registerValidator,
            IValidator<RegisterClinicDto> registerClinicValidator,
            IValidator<LoginDto> loginValidator,
            UsuarioRules usuarioRules,
            IUnitOfWork unitOfWork)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _rolRepository = rolRepository ?? throw new ArgumentNullException(nameof(rolRepository));
            _rolEspecialidadRepository = rolEspecialidadRepository ?? throw new ArgumentNullException(nameof(rolEspecialidadRepository));
            _especialidadRepository = especialidadRepository ?? throw new ArgumentNullException(nameof(especialidadRepository));
            _sucursalEspecialidadRepository = sucursalEspecialidadRepository ?? throw new ArgumentNullException(nameof(sucursalEspecialidadRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _registerValidator = registerValidator ?? throw new ArgumentNullException(nameof(registerValidator));
            _registerClinicValidator = registerClinicValidator ?? throw new ArgumentNullException(nameof(registerClinicValidator));
            _loginValidator = loginValidator ?? throw new ArgumentNullException(nameof(loginValidator));
            _usuarioRules = usuarioRules ?? throw new ArgumentNullException(nameof(usuarioRules));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<AuthResponseDto> RegisterClinicAsync(RegisterClinicDto registerClinicDto)
        {
            if (registerClinicDto == null)
                throw new ArgumentNullException(nameof(registerClinicDto));

            var validationResult = await _registerClinicValidator.ValidateAsync(registerClinicDto);
            if (!validationResult.IsValid)
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            Clinica? clinicaCreada = null;
            Usuario? usuarioCreado = null;
            Rol? rolAdmin = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                clinicaCreada = await _clinicaRepository.CrearAsync(new Clinica
                {
                    Nombre = registerClinicDto.NombreClinica.Trim(),
                    Telefono = registerClinicDto.TelefonoClinica.Trim(),
                    Direccion = registerClinicDto.DireccionClinica.Trim(),
                    Activa = true,
                    FechaCreacion = DateTime.UtcNow
                });

                if (clinicaCreada == null)
                {
                    throw new InvalidOperationException("No se pudo crear la clínica.");
                }

                var correoAdmin = registerClinicDto.Correo.Trim().ToLower();
                var correoUnico = await _usuarioRules.CorreoEsUnicoPorClinicaAsync(clinicaCreada.Id, correoAdmin);
                if (!correoUnico)
                {
                    throw new InvalidOperationException("Ya existe un usuario con ese correo en la clínica.");
                }

                rolAdmin = await _rolRepository.ObtenerPorNombreAsync(clinicaCreada.Id, RolAdminNombre)
                    ?? await _rolRepository.ObtenerPorNombreAsync(clinicaCreada.Id, "Administrador")
                    ?? await _rolRepository.CrearAsync(new Rol
                    {
                        IdClinica = clinicaCreada.Id,
                        IdSucursal = null,
                        Nombre = RolAdminNombre,
                        Descripcion = "Usuario global administrador",
                        Activo = true
                    });

                if (rolAdmin == null || !rolAdmin.Activo)
                {
                    throw new InvalidOperationException("No fue posible resolver el rol Admin para onboarding.");
                }

                var especialidadAdmin = await _especialidadRepository.ObtenerPorNombreAsync(clinicaCreada.Id, EspecialidadAdminNombre)
                    ?? await _especialidadRepository.CrearAsync(new Especialidad
                    {
                        IdClinica = clinicaCreada.Id,
                        Nombre = EspecialidadAdminNombre,
                        Descripcion = "Especialidad global para administradores",
                        Activa = true
                    });

                var rolEspecialidadAdmin = await _rolEspecialidadRepository.ObtenerActivaAsync(rolAdmin.Id, especialidadAdmin.Id);
                if (rolEspecialidadAdmin == null)
                {
                    await _rolEspecialidadRepository.CrearAsync(new RolEspecialidad
                    {
                        IdRol = rolAdmin.Id,
                        IdEspecialidad = especialidadAdmin.Id,
                        Activa = true
                    });
                }

                usuarioCreado = await _usuarioRepository.CrearAsync(new Usuario
                {
                    NombreCompleto = registerClinicDto.NombreCompleto.Trim(),
                    Correo = correoAdmin,
                    ClaveHash = _passwordHasher.Hash(registerClinicDto.Clave),
                    IdRol = rolAdmin.Id,
                    IdEspecialidad = especialidadAdmin.Id,
                    IdClinica = clinicaCreada.Id,
                    IdSucursal = null,
                    Activo = true,
                    DebeCambiarClave = false,
                    FechaCreacion = DateTime.UtcNow
                });

                if (usuarioCreado == null)
                {
                    throw new InvalidOperationException("No se pudo crear el administrador de la clínica.");
                }
            }, IsolationLevel.Serializable);

            if (clinicaCreada == null || usuarioCreado == null || rolAdmin == null)
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "No se pudo completar el registro de clínica y administrador"
                };
            }

            usuarioCreado.Rol = rolAdmin;
            usuarioCreado.Clinica = clinicaCreada;

            try
            {
                await _emailService.EnviarBienvenidaUsuarioAsync(
                    usuarioCreado.Correo,
                    usuarioCreado.NombreCompleto,
                    clinicaCreada.Nombre,
                    rolAdmin.Nombre);
            }
            catch
            {
                // No interrumpimos el onboarding si falla el correo.
            }

            var token = _jwtService.GenerarToken(usuarioCreado);
            var expiracion = _jwtService.ObtenerFechaExpiracion();
            var usuarioDto = MapToResponseDto(usuarioCreado);
            usuarioDto.NombreRol = rolAdmin.Nombre;

            return new AuthResponseDto
            {
                Exito = true,
                Mensaje = "Clínica y administrador registrados exitosamente",
                Token = token,
                Expiracion = expiracion,
                Usuario = usuarioDto
            };
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

            if (!await _usuarioRules.CorreoEsUnicoPorClinicaAsync(registerDto.IdClinica, registerDto.Correo))
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "Ya existe un usuario con este correo en la clínica"
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

            var esOnboardingAdmin = !registerDto.IdSucursal.HasValue && !registerDto.IdEspecialidad.HasValue;

            Rol? rol;
            if (esOnboardingAdmin)
            {
                rol = await _rolRepository.ObtenerPorNombreAsync(registerDto.IdClinica, RolAdminNombre)
                    ?? await _rolRepository.ObtenerPorNombreAsync(registerDto.IdClinica, "Administrador");

                if (rol == null)
                {
                    rol = await _rolRepository.CrearAsync(new Rol
                    {
                        IdClinica = registerDto.IdClinica,
                        IdSucursal = null,
                        Nombre = RolAdminNombre,
                        Descripcion = "Usuario global administrador",
                        Activo = true
                    });
                }
            }
            else
            {
                rol = await _rolRepository.ObtenerPorIdAsync(registerDto.IdRol);
            }

            if (rol == null || !rol.Activo)
            {
                return new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "El rol especificado no existe o está inactivo"
                };
            }

            var esAdminSistema = EsRolAdmin(rol.Nombre);
            var nombreRol = rol.Nombre;

            var usuario = new Usuario
            {
                NombreCompleto = registerDto.NombreCompleto.Trim(),
                Correo = registerDto.Correo.Trim().ToLower(),
                ClaveHash = _passwordHasher.Hash(registerDto.Clave),
                IdRol = rol.Id,
                IdEspecialidad = registerDto.IdEspecialidad,
                IdClinica = registerDto.IdClinica,
                IdSucursal = registerDto.IdSucursal,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            if (esAdminSistema)
            {
                // Admin no depende de sucursal; su especialidad global se crea/asigna por clínica.
                usuario.IdSucursal = null;

                var especialidadAdmin = await _especialidadRepository.ObtenerPorNombreAsync(registerDto.IdClinica, EspecialidadAdminNombre)
                    ?? await _especialidadRepository.CrearAsync(new Especialidad
                    {
                        IdClinica = registerDto.IdClinica,
                        Nombre = EspecialidadAdminNombre,
                        Descripcion = "Especialidad global para administradores",
                        Activa = true
                    });

                usuario.IdEspecialidad = especialidadAdmin.Id;

                var rolEspecialidadAdmin = await _rolEspecialidadRepository.ObtenerActivaAsync(rol.Id, especialidadAdmin.Id);
                if (rolEspecialidadAdmin == null)
                {
                    await _rolEspecialidadRepository.CrearAsync(new RolEspecialidad
                    {
                        IdRol = rol.Id,
                        IdEspecialidad = especialidadAdmin.Id,
                        Activa = true
                    });
                }

                usuario.DebeCambiarClave = false;
            }
            else
            {
                if (!registerDto.IdSucursal.HasValue)
                {
                    return new AuthResponseDto
                    {
                        Exito = false,
                        Mensaje = "Los roles operativos requieren una sucursal asignada"
                    };
                }

                if (!registerDto.IdEspecialidad.HasValue)
                {
                    return new AuthResponseDto
                    {
                        Exito = false,
                        Mensaje = "Los roles operativos requieren especialidad"
                    };
                }

                var especialidadActiva = await _especialidadRepository.ExisteActivaAsync(registerDto.IdClinica, registerDto.IdEspecialidad.Value);
                var especialidadEnSucursal = await _sucursalEspecialidadRepository.ObtenerConfiguracionActivaAsync(
                    registerDto.IdSucursal.Value,
                    registerDto.IdEspecialidad.Value);
                var especialidadEnRol = await _rolEspecialidadRepository.ObtenerActivaAsync(rol.Id, registerDto.IdEspecialidad.Value);

                if (!especialidadActiva || especialidadEnSucursal == null || especialidadEnRol == null)
                {
                    return new AuthResponseDto
                    {
                        Exito = false,
                        Mensaje = "La especialidad no está activa para la sucursal o no está asignada al rol"
                    };
                }

                usuario.DebeCambiarClave = true;
            }

            var usuarioCreado = await _usuarioRepository.CrearAsync(usuario);
            usuarioCreado.Rol = rol;

            var clinica = await _clinicaRepository.ObtenerPorIdAsync(usuarioCreado.IdClinica);
            var nombreClinica = clinica?.Nombre ?? "Clínica asignada";

            await _emailService.EnviarBienvenidaUsuarioAsync(
                usuarioCreado.Correo,
                usuarioCreado.NombreCompleto,
                nombreClinica,
                nombreRol);

            var token = _jwtService.GenerarToken(usuarioCreado);
            var expiracion = _jwtService.ObtenerFechaExpiracion();
            var usuarioDto = MapToResponseDto(usuarioCreado);
            usuarioDto.NombreRol = nombreRol;

            return new AuthResponseDto
            {
                Exito = true,
                Mensaje = "Usuario registrado exitosamente",
                Token = token,
                Expiracion = expiracion,
                Usuario = usuarioDto
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

        private static bool RolRequiereEspecialidad(string nombreRol)
        {
            return nombreRol.Equals("Doctor", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsRolAdmin(string? nombreRol)
        {
            return string.Equals(nombreRol?.Trim(), RolAdminNombre, StringComparison.OrdinalIgnoreCase)
                || string.Equals(nombreRol?.Trim(), "Administrador", StringComparison.OrdinalIgnoreCase);
        }
    }
}