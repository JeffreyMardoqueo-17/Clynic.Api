using Clynic.Application.DTOs.Citas;

namespace Clynic.Application.DTOs.Pacientes
{
    public class PacienteResponseDto
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; }
        public HistorialClinicoResponseDto? HistorialClinico { get; set; }
        public IReadOnlyCollection<ConsultaMedicaResponseDto> ConsultasRecientes { get; set; }
            = Array.Empty<ConsultaMedicaResponseDto>();
    }

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
