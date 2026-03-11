using System.ComponentModel.DataAnnotations;
namespace Clynic.Application.DTOs.Usuarios
{
    public class UpdateUsuarioDto
    {
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        public string? NombreCompleto { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
        public string? Correo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID de la sucursal debe ser mayor a 0")]
        public int? IdSucursal { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID del rol debe ser mayor a 0")]
        public int? IdRol { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID de la especialidad debe ser mayor a 0")]
        public int? IdEspecialidad { get; set; }

        public bool? Activo { get; set; }
    }
}
