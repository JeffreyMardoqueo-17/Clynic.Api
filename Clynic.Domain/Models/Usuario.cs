using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clynic.Domain.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string ClaveHash { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        ///propieadades de navegacion con clinica
        public ICollection<Clinica>? Clinicas { get; set; } = new List<Clinica>();
    }
}