namespace Clynic.Application.DTOs.LandingPages
{
    public class LandingHorarioDto
    {
        public int DiaSemana { get; set; }
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
    }
}
