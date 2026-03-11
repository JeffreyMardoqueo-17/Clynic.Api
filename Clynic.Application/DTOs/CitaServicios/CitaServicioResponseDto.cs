namespace Clynic.Application.DTOs.CitaServicios
{
    public class CitaServicioResponseDto
    {
        public int Id { get; set; }
        public int IdCita { get; set; }
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public int DuracionMin { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaHoraInicioPlanCita { get; set; }
        public DateTime FechaHoraFinPlanCita { get; set; }
        public decimal SubTotalCita { get; set; }
        public decimal TotalFinalCita { get; set; }
    }
}
