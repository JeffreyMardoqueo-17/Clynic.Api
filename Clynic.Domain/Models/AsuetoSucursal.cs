namespace Clynic.Domain.Models
{
    public class AsuetoSucursal
    {
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        public DateOnly Fecha { get; set; }
        public string? Motivo { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}