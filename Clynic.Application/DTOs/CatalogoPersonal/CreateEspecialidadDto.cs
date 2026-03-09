using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.CatalogoPersonal
{
    public class CreateEspecialidadDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdClinica { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(400)]
        public string Descripcion { get; set; } = string.Empty;
    }
}
