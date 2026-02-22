using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Usuarios
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "La clave actual es obligatoria")]
        public string ClaveActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva clave debe tener entre 6 y 100 caracteres")]
        public string NuevaClave { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de clave es obligatoria")]
        [Compare("NuevaClave", ErrorMessage = "Las claves no coinciden")]
        public string ConfirmarClave { get; set; } = string.Empty;
    }
}
