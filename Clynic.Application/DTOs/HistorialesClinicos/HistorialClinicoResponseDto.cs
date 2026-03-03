namespace Clynic.Application.DTOs.HistorialesClinicos
{
    public class HistorialClinicoResponseDto
    {
        public int Id { get; set; }
        public int IdPaciente { get; set; }
        public string EnfermedadesPrevias { get; set; } = string.Empty;
        public string MedicamentosActuales { get; set; } = string.Empty;
        public string Alergias { get; set; } = string.Empty;
        public string AntecedentesFamiliares { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
