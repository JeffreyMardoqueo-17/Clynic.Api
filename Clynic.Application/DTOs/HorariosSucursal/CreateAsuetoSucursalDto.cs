namespace Clynic.Application.DTOs.HorariosSucursal
{
    public class CreateAsuetoSucursalDto
    {
        public int IdSucursal { get; set; }
        public DateOnly Fecha { get; set; }
        public string? Motivo { get; set; }
    }
}