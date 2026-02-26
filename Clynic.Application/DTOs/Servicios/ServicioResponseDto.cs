namespace Clynic.Application.DTOs.Servicios
{
    public class ServicioResponseDto
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public int DuracionMin { get; set; }
        public decimal PrecioBase { get; set; }
        public bool Activo { get; set; }
    }
}
