using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clynic.Domain.Models
{
    public class Paciente
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        [NotMapped]
        public string NombreCompleto
        {
            get => $"{Nombres} {Apellidos}".Trim();
            set
            {
                var valor = value?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(valor))
                {
                    Nombres = string.Empty;
                    Apellidos = string.Empty;
                    return;
                }

                var partes = valor.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                Nombres = partes[0];
                Apellidos = partes.Length > 1 ? partes[1] : string.Empty;
            }
        }
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public HistorialClinico? HistorialClinico { get; set; }
        public ICollection<ConsultaMedica> ConsultasMedicas { get; set; }
            = new List<ConsultaMedica>();
        public ICollection<Cita> Citas { get; set; }
            = new List<Cita>();
    }
}
