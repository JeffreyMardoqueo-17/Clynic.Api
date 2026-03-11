namespace Clynic.Application.DTOs.Servicios
{
    public class CapacidadEspecialidadDiaDto
    {
        public DateTime Fecha { get; set; }
        public int IdEspecialidad { get; set; }
        public string NombreEspecialidad { get; set; } = string.Empty;
        public int TotalCitasAgendadas { get; set; }
        public int TotalMinutosAgendados { get; set; }
        public decimal DuracionPromedioCitaMin { get; set; }
        public int MinutosLaborablesDia { get; set; }
        public int MinutosDisponiblesDia { get; set; }
        public int CitasPosiblesDia { get; set; }
        public int CitasDisponiblesDia { get; set; }
        public decimal SaturacionPct { get; set; }
    }
}
