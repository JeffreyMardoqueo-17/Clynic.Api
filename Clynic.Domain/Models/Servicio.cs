using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clynic.Domain.Models
{
    public class Servicio
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public int DuracionMin { get; set; }
        public decimal PrecioBase { get; set; }
        public bool Activo { get; set; } = true;
        public ICollection<CitaServicio> CitaServicios { get; set; }
            = new List<CitaServicio>();
    }
}
