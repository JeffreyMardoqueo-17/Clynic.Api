namespace Clynic.Domain.Models
{
    public class RolEspecialidad
    {
        public int Id { get; set; }
        public int IdRol { get; set; }
        public Rol? Rol { get; set; }
        public int IdEspecialidad { get; set; }
        public Especialidad? Especialidad { get; set; }
        public bool Activa { get; set; } = true;
    }
}
