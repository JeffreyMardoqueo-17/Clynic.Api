using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clynic.Domain.Models.Enums;

namespace Clynic.Domain.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        public int IdPaciente { get; set; }
        public Paciente? Paciente { get; set; }
        public int? IdDoctor { get; set; }
        public Usuario? Doctor { get; set; }
        public DateTime FechaHoraInicioPlan { get; set; }
        public DateTime FechaHoraFinPlan { get; set; }
        public DateTime? FechaHoraInicioReal { get; set; }
        public DateTime? FechaHoraFinReal { get; set; }
        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;
        public string Notas { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TotalFinal { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public ConsultaMedica? ConsultaMedica { get; set; }
        public ICollection<CitaServicio> CitaServicios { get; set; }
            = new List<CitaServicio>();
    }
}
