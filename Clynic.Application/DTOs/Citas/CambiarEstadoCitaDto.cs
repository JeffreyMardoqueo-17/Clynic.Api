using Clynic.Domain.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Citas
{
    public class CambiarEstadoCitaDto
    {
        [Required]
        public EstadoCita NuevoEstado { get; set; }

        [StringLength(250)]
        public string NotasOperacion { get; set; } = string.Empty;
    }
}
