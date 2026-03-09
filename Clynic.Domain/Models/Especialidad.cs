namespace Clynic.Domain.Models
{
    public class Especialidad
    {
        public int Id { get; set; }
        public int? IdClinica { get; set; }
        public Clinica? Clinica { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
        public ICollection<SucursalEspecialidad> SucursalesEspecialidad { get; set; } = new List<SucursalEspecialidad>();
        public ICollection<RolEspecialidad> RolesEspecialidad { get; set; } = new List<RolEspecialidad>();
    }
}
