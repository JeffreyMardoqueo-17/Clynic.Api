namespace Clynic.Application.DTOs.HorariosSucursal
{
    public class UpdateHorarioSucursalDto
    {
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}