using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Citas
{
    public class ReprogramarCitaDto
    {
        [Required]
        public DateTime NuevaFechaHoraInicioPlan { get; set; }

        [StringLength(250)]
        public string Motivo { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int? IdDoctor { get; set; }
    }
}
