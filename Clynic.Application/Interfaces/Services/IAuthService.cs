using Clynic.Application.DTOs.Usuarios;

namespace Clynic.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegistrarAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
}
