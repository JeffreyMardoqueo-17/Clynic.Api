using Clynic.Application.DTOs.Usuarios;
using Clynic.Domain.Models.Enums;

namespace Clynic.Application.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioResponseDto>> ObtenerTodosAsync();
        Task<IEnumerable<UsuarioResponseDto>> ObtenerPorClinicaAsync(int idClinica);
        Task<UsuarioResponseDto> ObtenerPerfilAsync(int idUsuario);
        Task<UsuarioResponseDto?> ObtenerPorIdAsync(int id);
        Task<UsuarioResponseDto?> ObtenerPorCorreoAsync(string correo);
        Task<UsuarioResponseDto> CrearAsync(RegisterDto createDto);
        Task<UsuarioResponseDto?> ActualizarAsync(int id, UpdateUsuarioDto updateDto);
        Task<bool> EliminarAsync(int id);
        Task<bool> CambiarClaveAsync(int id, ChangePasswordDto changePasswordDto);
        Task<bool> ActualizarClaveAsync(int id, string nuevaClaveHash);
    }
}
