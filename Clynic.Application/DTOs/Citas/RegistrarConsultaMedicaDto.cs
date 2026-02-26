using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Citas
{
    public class RegistrarConsultaMedicaDto
    {
        [Required]
        [StringLength(4000, MinimumLength = 3)]
        public string Diagnostico { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Tratamiento { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Receta { get; set; } = string.Empty;

        [StringLength(4000)]
        public string ExamenesSolicitados { get; set; } = string.Empty;

        [StringLength(4000)]
        public string NotasMedicas { get; set; } = string.Empty;

        public DateTime? FechaConsulta { get; set; }
    }
}
