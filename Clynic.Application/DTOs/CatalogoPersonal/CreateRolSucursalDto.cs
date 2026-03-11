using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.CatalogoPersonal
{
    public class CreateRolSucursalDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdClinica { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdSucursal { get; set; }

        [Required]
        [StringLength(80, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string Descripcion { get; set; } = string.Empty;
    }
}
