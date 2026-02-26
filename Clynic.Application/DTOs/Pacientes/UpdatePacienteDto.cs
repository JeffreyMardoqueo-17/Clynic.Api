using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Pacientes
{
    public class UpdatePacienteDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(50)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        public DateTime? FechaNacimiento { get; set; }
    }
}
