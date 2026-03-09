using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Usuarios
{
    public class RegisterClinicDto
    {
        [Required(ErrorMessage = "El nombre de la clínica es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre de la clínica debe tener entre 3 y 150 caracteres")]
        public string NombreClinica { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono de la clínica es obligatorio")]
        [StringLength(50, MinimumLength = 7, ErrorMessage = "El teléfono de la clínica debe tener entre 7 y 50 caracteres")]
        public string TelefonoClinica { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección de la clínica es obligatoria")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "La dirección de la clínica debe tener entre 5 y 250 caracteres")]
        public string DireccionClinica { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener entre 6 y 100 caracteres")]
        public string Clave { get; set; } = string.Empty;
    }
}
