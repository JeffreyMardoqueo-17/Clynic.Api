namespace Clynic.Domain.Models
{
    public class ConsultaMedica
    {
        public int Id { get; set; }
        public int IdCita { get; set; }
        public Cita? Cita { get; set; }
        public int IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        public int IdPaciente { get; set; }
        public Paciente? Paciente { get; set; }
        public int? IdDoctor { get; set; }
        public Usuario? Doctor { get; set; }
        public string Diagnostico { get; set; } = string.Empty;
        public string Tratamiento { get; set; } = string.Empty;
        public string Receta { get; set; } = string.Empty;
        public string ExamenesSolicitados { get; set; } = string.Empty;
        public string NotasMedicas { get; set; } = string.Empty;
        public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
