namespace Clynic.Application.DTOs.HorariosSucursal
{
    /// <summary>
    /// DTO para crear un horario de sucursal
    /// </summary>
    public class CreateHorarioSucursalDto
    {
        public int IdSucursal { get; set; }

        public int DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }
    }
}
