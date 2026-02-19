using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clynic.Domain.Models
{
    public class CitaServicio
    {
        public int Id { get; set; }
        public int IdCita { get; set; }
        public Cita? Cita { get; set; }
        public int IdServicio { get; set; }
        public Servicio? Servicio { get; set; }
        public int DuracionMin { get; set; }
        public decimal Precio { get; set; }
    }
}
