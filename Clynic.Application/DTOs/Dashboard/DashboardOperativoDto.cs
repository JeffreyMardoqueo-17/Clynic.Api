namespace Clynic.Application.DTOs.Dashboard
{
    public class DashboardCitaPendienteDto
    {
        public int IdCita { get; set; }
        public int IdSucursal { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public DateTime FechaHoraInicioPlan { get; set; }
        public DateTime FechaHoraFinPlan { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class DashboardOperativoDto
    {
        public int TotalPacientes { get; set; }
        public int TotalCitasHoy { get; set; }
        public int TotalCitasPendientes { get; set; }
        public int TotalTrabajadores { get; set; }
        public bool FiltradoPorSucursal { get; set; }
        public int? IdSucursal { get; set; }
        public IReadOnlyList<DashboardCitaPendienteDto> Pendientes { get; set; } = Array.Empty<DashboardCitaPendienteDto>();
    }
}
