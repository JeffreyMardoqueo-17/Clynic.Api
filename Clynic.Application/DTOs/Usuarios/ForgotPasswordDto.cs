using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Usuarios
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Correo { get; set; } = string.Empty;
    }
}
