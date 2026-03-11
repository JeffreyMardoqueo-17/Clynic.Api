namespace Clynic.Application.DTOs.LandingPages
{
    public class LandingServicioClinicaDto
    {
        public int Id { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public int DuracionMin { get; set; }
    }
}
