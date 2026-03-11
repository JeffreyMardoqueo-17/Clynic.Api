namespace Clynic.Application.DTOs.Usuarios
{
    public class UsuarioResponseDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string DescripcionRol { get; set; } = string.Empty;
        public int? IdEspecialidad { get; set; }
        public string? NombreEspecialidad { get; set; }
        public bool Activo { get; set; }
        public bool DebeCambiarClave { get; set; }
        public int IdClinica { get; set; }
        public string? NombreClinica { get; set; }
        public int? IdSucursal { get; set; }
        public string? NombreSucursal { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
