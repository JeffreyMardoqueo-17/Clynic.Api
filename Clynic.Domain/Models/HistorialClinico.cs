namespace Clynic.Domain.Models
{
    public class HistorialClinico
    {
        public int Id { get; set; }
        public int IdPaciente { get; set; }
        public Paciente? Paciente { get; set; }
        public string EnfermedadesPrevias { get; set; } = string.Empty;
        public string MedicamentosActuales { get; set; } = string.Empty;
        public string Alergias { get; set; } = string.Empty;
        public string AntecedentesFamiliares { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}
