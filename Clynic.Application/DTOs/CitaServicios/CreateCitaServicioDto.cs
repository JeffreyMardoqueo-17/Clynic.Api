using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.CitaServicios
{
    public class CreateCitaServicioDto
    {
        [Range(1, int.MaxValue)]
        public int IdCita { get; set; }

        [Range(1, int.MaxValue)]
        public int IdServicio { get; set; }

        [Range(1, 1440)]
        public int? DuracionMin { get; set; }

        [Range(0, 999999.99)]
        public decimal? Precio { get; set; }
    }
}
