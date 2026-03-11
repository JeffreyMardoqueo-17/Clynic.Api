using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.CatalogoPersonal
{
    public class AsignarEspecialidadSucursalDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdClinica { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdSucursal { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdEspecialidad { get; set; }
    }
}
