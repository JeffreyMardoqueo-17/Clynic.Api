using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clynic.Domain.Models
{
    public class Clinica
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public ICollection<Sucursal> Sucursales { get; set; }
            = new List<Sucursal>();
        public ICollection<Usuario> Usuarios { get; set; }
            = new List<Usuario>();
        public ICollection<Servicio> Servicios { get; set; }
            = new List<Servicio>();
        public ICollection<Cita> Citas { get; set; }
            = new List<Cita>();
    }
}