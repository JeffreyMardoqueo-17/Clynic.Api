using System;
using System.Collections.Generic;
namespace Clynic.Domain.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public int? IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string ClaveHash { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public Rol? Rol { get; set; }
        public int? IdEspecialidad { get; set; }
        public Especialidad? Especialidad { get; set; }
        public bool Activo { get; set; } = true;
        public bool DebeCambiarClave { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public ICollection<Cita> CitasComoDoctor { get; set; }
            = new List<Cita>();
        public ICollection<ConsultaMedica> ConsultasMedicasRealizadas { get; set; }
            = new List<ConsultaMedica>();
    }
}