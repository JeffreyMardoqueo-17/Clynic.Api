namespace Clynic.Application.DTOs.Citas
{
    public class HorariosDisponiblesCitaDto
    {
        public int IdClinica { get; set; }
        public int IdSucursal { get; set; }
        public int IdEspecialidad { get; set; }
        public DateTime Fecha { get; set; }
        public int DuracionEstimadaMin { get; set; }
        public int IntervaloMin { get; set; }
        public int CitasMaximasPorDiaEspecialidad { get; set; }
        public int CitasOcupadasDiaEspecialidad { get; set; }
        public IReadOnlyCollection<HorarioDisponibleItemDto> Horarios { get; set; } = Array.Empty<HorarioDisponibleItemDto>();
    }

    public class HorarioDisponibleItemDto
    {
        public DateTime FechaHoraInicioPlan { get; set; }
        public DateTime FechaHoraFinPlan { get; set; }
        public string HoraLabel { get; set; } = string.Empty;
    }
}
