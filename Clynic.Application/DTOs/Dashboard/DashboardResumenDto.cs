namespace Clynic.Application.DTOs.Dashboard
{
    public class DashboardResumenDto
    {
        public int TotalPacientes { get; set; }
        public int TotalCitasHoy { get; set; }
        public int TotalTrabajadores { get; set; }
        public int TotalSucursales { get; set; }
        public DateTime FechaCorte { get; set; }
    }
}
