using System;
using System.Collections.Generic;
using Clynic.Domain.Models.Enums;

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
        public UsuarioRol Rol { get; set; } = UsuarioRol.Admin;
        public bool Activo { get; set; } = true;
        public bool DebeCambiarClave { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public ICollection<Cita> CitasComoDoctor { get; set; }
            = new List<Cita>();
        public ICollection<ConsultaMedica> ConsultasMedicasRealizadas { get; set; }
            = new List<ConsultaMedica>();
    }
}