using System.ComponentModel.DataAnnotations;

namespace Clynic.Application.DTOs.Citas
{
    public class AsignarDoctorCitaDto
    {
        [Range(1, int.MaxValue)]
        public int? IdDoctor { get; set; }
    }
}
