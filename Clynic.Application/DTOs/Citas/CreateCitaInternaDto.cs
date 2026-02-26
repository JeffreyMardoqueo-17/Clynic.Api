using System.ComponentModel.DataAnnotations;
using Clynic.Domain.Models.Enums;

namespace Clynic.Application.DTOs.Citas
{
    public class CreateCitaInternaDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdClinica { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdSucursal { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdPaciente { get; set; }

        [Range(1, int.MaxValue)]
        public int? IdDoctor { get; set; }

        [Required]
        public DateTime FechaHoraInicioPlan { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> IdsServicios { get; set; } = new();

        [StringLength(250)]
        public string Notas { get; set; } = string.Empty;

        public EstadoCita EstadoInicial { get; set; } = EstadoCita.Confirmada;
    }
}
