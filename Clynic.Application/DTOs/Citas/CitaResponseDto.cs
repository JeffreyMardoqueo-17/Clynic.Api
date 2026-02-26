using Clynic.Domain.Models.Enums;

namespace Clynic.Application.DTOs.Citas
{
    public class CitaResponseDto
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public int IdSucursal { get; set; }
        public int IdPaciente { get; set; }
        public int? IdDoctor { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public string CorreoPaciente { get; set; } = string.Empty;
        public string TelefonoPaciente { get; set; } = string.Empty;
        public DateTime FechaHoraInicioPlan { get; set; }
        public DateTime FechaHoraFinPlan { get; set; }
        public DateTime? FechaHoraInicioReal { get; set; }
        public DateTime? FechaHoraFinReal { get; set; }
        public EstadoCita Estado { get; set; }
        public string Notas { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TotalFinal { get; set; }
        public DateTime FechaCreacion { get; set; }
        public IReadOnlyCollection<CitaServicioDetalleDto> Servicios { get; set; } = Array.Empty<CitaServicioDetalleDto>();
        public ConsultaMedicaResponseDto? ConsultaMedica { get; set; }
    }

    public class CitaServicioDetalleDto
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public int DuracionMin { get; set; }
        public decimal Precio { get; set; }
    }

    public class ConsultaMedicaResponseDto
    {
        public int Id { get; set; }
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public int? IdDoctor { get; set; }
        public string Diagnostico { get; set; } = string.Empty;
        public string Tratamiento { get; set; } = string.Empty;
        public string Receta { get; set; } = string.Empty;
        public string ExamenesSolicitados { get; set; } = string.Empty;
        public string NotasMedicas { get; set; } = string.Empty;
        public DateTime FechaConsulta { get; set; }
    }
}
