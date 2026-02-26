using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Citas
{
    public class CreateCitaPublicaDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdClinica { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdSucursal { get; set; }

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

        [Required]
        public DateTime FechaHoraInicioPlan { get; set; }

        [StringLength(250)]
        public string Notas { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<int> IdsServicios { get; set; } = new();
    }
}
