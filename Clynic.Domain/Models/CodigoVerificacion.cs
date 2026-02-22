namespace Clynic.Domain.Models
{
    public class CodigoVerificacion
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public TipoCodigo Tipo { get; set; } = TipoCodigo.CambioContrasena;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; } = false;
        public DateTime? FechaUso { get; set; }
    }
}
