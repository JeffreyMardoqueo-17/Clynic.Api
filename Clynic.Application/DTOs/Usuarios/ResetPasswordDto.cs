using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Usuarios
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de verificación es obligatorio")]
        [StringLength(12, MinimumLength = 8, ErrorMessage = "El código debe tener entre 8 y 12 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva clave debe tener entre 6 y 100 caracteres")]
        public string NuevaClave { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de clave es obligatoria")]
        [Compare("NuevaClave", ErrorMessage = "Las claves no coinciden")]
        public string ConfirmarClave { get; set; } = string.Empty;
    }
}
