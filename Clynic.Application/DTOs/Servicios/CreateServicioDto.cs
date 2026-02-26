using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Servicios
{
    public class CreateServicioDto
    {
        [Required(ErrorMessage = "El ID de la clínica es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la clínica debe ser mayor a 0")]
        public int IdClinica { get; set; }

        [Required(ErrorMessage = "El nombre del servicio es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        public string NombreServicio { get; set; } = string.Empty;

        [Required(ErrorMessage = "La duración es obligatoria")]
        [Range(1, 600, ErrorMessage = "La duración debe estar entre 1 y 600 minutos")]
        public int DuracionMin { get; set; }

        [Range(typeof(decimal), "0", "9999999.99", ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal PrecioBase { get; set; }
    }
}
