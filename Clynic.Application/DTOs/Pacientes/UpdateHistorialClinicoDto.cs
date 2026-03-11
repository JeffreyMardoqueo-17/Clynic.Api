using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Pacientes
{
    public class UpdateHistorialClinicoDto
    {
        [StringLength(4000)]
        public string EnfermedadesPrevias { get; set; } = string.Empty;

        [StringLength(4000)]
        public string MedicamentosActuales { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Alergias { get; set; } = string.Empty;

        [StringLength(4000)]
        public string AntecedentesFamiliares { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Observaciones { get; set; } = string.Empty;
    }
}
