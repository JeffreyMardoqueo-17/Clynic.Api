namespace Clynic.Domain.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public int? IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public int? IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<RolEspecialidad> RolesEspecialidad { get; set; } = new List<RolEspecialidad>();
    }
}
