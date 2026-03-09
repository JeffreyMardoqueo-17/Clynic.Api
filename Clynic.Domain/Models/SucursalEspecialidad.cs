namespace Clynic.Domain.Models
{
    public class SucursalEspecialidad
    {
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        public int IdEspecialidad { get; set; }
        public Especialidad? Especialidad { get; set; }
        public bool Activa { get; set; } = true;
    }
}
