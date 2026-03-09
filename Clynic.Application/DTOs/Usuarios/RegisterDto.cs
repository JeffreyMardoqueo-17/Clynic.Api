using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Clynic.Application.DTOs.Usuarios
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener entre 6 y 100 caracteres")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID de la clínica es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la clínica debe ser mayor a 0")]
        public int IdClinica { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El ID de la sucursal debe ser mayor a 0")]
        public int? IdSucursal { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del rol debe ser mayor a 0")]
        public int IdRol { get; set; }

        // Compatibilidad con clientes antiguos que envian `rol` en lugar de `idRol`.
        [JsonPropertyName("rol")]
        public int? RolCompat
        {
            get => null;
            set
            {
                if (!value.HasValue)
                {
                    return;
                }

                if (IdRol <= 0)
                {
                    IdRol = value.Value;
                }
            }
        }

        [Range(1, int.MaxValue, ErrorMessage = "El ID de la especialidad debe ser mayor a 0")]
        public int? IdEspecialidad { get; set; }
    }
}
