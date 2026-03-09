namespace Clynic.Application.DTOs.CatalogoPersonal
{
    public class EspecialidadSucursalResponseDto
    {
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public int IdEspecialidad { get; set; }
        public string NombreEspecialidad { get; set; } = string.Empty;
        public bool Activa { get; set; }
    }
}
