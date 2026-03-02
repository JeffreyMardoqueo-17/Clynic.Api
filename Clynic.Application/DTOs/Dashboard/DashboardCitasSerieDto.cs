namespace Clynic.Application.DTOs.Dashboard
{
    public class DashboardCitasPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public int TotalCitas { get; set; }
    }

    public class DashboardCitasSerieDto
    {
        public DateTime FechaMinima { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int TotalPeriodo { get; set; }
        public IReadOnlyList<DashboardCitasPorDiaDto> Serie { get; set; } = Array.Empty<DashboardCitasPorDiaDto>();
    }
}
