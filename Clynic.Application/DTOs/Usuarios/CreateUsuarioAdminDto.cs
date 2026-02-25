using System.ComponentModel.DataAnnotations;
using Clynic.Domain.Models.Enums;

namespace Clynic.Application.DTOs.Usuarios
{
    public class CreateUsuarioAdminDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID de la clínica es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la clínica debe ser mayor a 0")]
        public int IdClinica { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID de la sucursal debe ser mayor a 0")]
        public int? IdSucursal { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public UsuarioRol Rol { get; set; } = UsuarioRol.Doctor;
    }
}
