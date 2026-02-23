namespace Clynic.Application.DTOs.Usuarios
{
    public class AuthResponseDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public UsuarioResponseDto? Usuario { get; set; }
    }
}
