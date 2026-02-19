using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clynic.Domain.Models
{
    public class HorarioSucursal
    {
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFin { get; set; }
    }
}
