using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.CatalogoPersonal
{
    public class ActualizarEstadoEspecialidadSucursalesDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdClinica { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdEspecialidad { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> IdsSucursales { get; set; } = new();

        [Required]
        public bool Activa { get; set; }
    }
}
