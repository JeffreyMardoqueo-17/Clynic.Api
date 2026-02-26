namespace Clynic.Application.DTOs.HorariosSucursal
{
    public class AsuetoSucursalResponseDto
    {
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public DateOnly Fecha { get; set; }
        public string? Motivo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}