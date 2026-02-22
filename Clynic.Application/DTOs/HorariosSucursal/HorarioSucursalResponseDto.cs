namespace Clynic.Application.DTOs.HorariosSucursal
{
    /// <summary>
    /// DTO de respuesta para horarios de sucursal
    /// </summary>
    public class HorarioSucursalResponseDto
    {
        public int Id { get; set; }

        public int IdSucursal { get; set; }

        public int DiaSemana { get; set; }

        public TimeSpan? HoraInicio { get; set; }

        public TimeSpan? HoraFin { get; set; }
    }
}
