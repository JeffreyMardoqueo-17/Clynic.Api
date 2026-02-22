namespace Clynic.Application.DTOs.Usuarios
{
    public class AuthResponseDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? Expiracion { get; set; }
        public UsuarioResponseDto? Usuario { get; set; }
    }
}
