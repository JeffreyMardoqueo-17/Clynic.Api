using Clynic.Application.DTOs.Usuarios;

namespace Clynic.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterClinicAsync(RegisterClinicDto registerClinicDto);
        Task<AuthResponseDto> RegistrarAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
}