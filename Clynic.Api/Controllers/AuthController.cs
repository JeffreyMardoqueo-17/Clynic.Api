using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Clynic.Application.DTOs.Usuarios;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using System.Security.Claims;

namespace Clynic.Api.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;
        private readonly IUsuarioService _usuarioService;
        private readonly IEmailService _emailService;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthController(
            IAuthService authService,
            IUsuarioService usuarioService,
            IEmailService emailService,
            IVerificationCodeService verificationCodeService,
            IPasswordHasher passwordHasher,
            IWebHostEnvironment env)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _verificationCodeService = verificationCodeService ?? throw new ArgumentNullException(nameof(verificationCodeService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "Los datos de registro son requeridos"
                });
            }

            var resultado = await _authService.RegistrarAsync(registerDto);

            if (!resultado.Exito)
            {
                return BadRequest(resultado);
            }

            SetAuthCookie(resultado);

            return Ok(resultado);
        }

        // ==========================
        // LOGIN
        // ==========================
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Exito = false,
                    Mensaje = "Las credenciales son requeridas"
                });
            }

            var resultado = await _authService.LoginAsync(loginDto);

            if (!resultado.Exito)
            {
                return Unauthorized(resultado);
            }

            SetAuthCookie(resultado);

            return Ok(resultado);
        }
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (forgotPasswordDto == null)
            {
                return BadRequest(new { mensaje = "El correo es requerido" });
            }

            var usuario = await _usuarioService.ObtenerPorCorreoAsync(forgotPasswordDto.Correo);

            if (usuario == null)
            {
                return Ok(new { mensaje = "Si el correo existe, recibirás un código de verificación" });
            }

            if (!usuario.Activo)
            {
                return Ok(new { mensaje = "Si el correo existe, recibirás un código de verificación" });
            }

            var codigoVerificacion = await _verificationCodeService.CrearCodigoAsync(
                usuario.Id,
                TipoCodigo.CambioContrasena,
                15
            );

            try
            {
                await _emailService.EnviarCodigoVerificacionAsync(
                    usuario.Correo,
                    usuario.NombreCompleto,
                    codigoVerificacion.Codigo
                );
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Error al enviar el correo. Intenta nuevamente." });
            }

            return Ok(new { mensaje = "Si el correo existe, recibirás un código de verificación" });
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto == null)
            {
                return BadRequest(new { mensaje = "Los datos son requeridos" });
            }

            var usuario = await _usuarioService.ObtenerPorCorreoAsync(resetPasswordDto.Correo);

            if (usuario == null)
            {
                return BadRequest(new { mensaje = "Código inválido o expirado" });
            }

            var codigoValido = await _verificationCodeService.ValidarCodigoAsync(
                usuario.Id,
                resetPasswordDto.Codigo,
                TipoCodigo.CambioContrasena
            );

            if (codigoValido == null)
            {
                return BadRequest(new { mensaje = "Código inválido o expirado" });
            }

            await _verificationCodeService.MarcarComoUsadoAsync(codigoValido);

            var nuevaClaveHash = _passwordHasher.Hash(resetPasswordDto.NuevaClave);
            await _usuarioService.ActualizarClaveAsync(usuario.Id, nuevaClaveHash);

            return Ok(new { mensaje = "Contraseña actualizada exitosamente" });
        }

        [HttpGet("me")]
        // [Authorize]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioResponseDto>> Me()
        {
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(usuarioIdClaim, out var usuarioId) || usuarioId <= 0)
                return Unauthorized(new { mensaje = "Token inválido" });

            try
            {
                var perfil = await _usuarioService.ObtenerPerfilAsync(usuarioId);
                return Ok(perfil);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { mensaje = "Usuario no encontrado" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { mensaje = "Usuario inactivo" });
            }
        }

        private void SetAuthCookie(AuthResponseDto authResult)
        {
            if (string.IsNullOrWhiteSpace(authResult.Token))
                return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = authResult.Expiracion ?? DateTime.UtcNow.AddHours(24)
            };

            Response.Cookies.Append("clynic_access_token", authResult.Token, cookieOptions);
        }
    }
}
