namespace Clynic.Application.DTOs.CatalogoPersonal
{
    public class RolSucursalResponseDto
    {
        public int Id { get; set; }
        public int? IdClinica { get; set; }
        public int? IdSucursal { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
