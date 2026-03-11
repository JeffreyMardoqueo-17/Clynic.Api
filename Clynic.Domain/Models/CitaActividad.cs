namespace Clynic.Domain.Models
{
    public class CitaActividad
    {
        public int Id { get; set; }
        public int IdCita { get; set; }
        public Cita? Cita { get; set; }
        public int IdClinica { get; set; }
        public int IdSucursal { get; set; }
        public int? IdUsuario { get; set; }
        public string RolUsuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
