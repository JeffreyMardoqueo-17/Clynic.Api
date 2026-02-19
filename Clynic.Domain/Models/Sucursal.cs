using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clynic.Domain.Models
{
    public class Sucursal
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public ICollection<HorarioSucursal> Horarios { get; set; }
            = new List<HorarioSucursal>();
        public ICollection<Cita> Citas { get; set; }
            = new List<Cita>();
    }
}