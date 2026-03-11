namespace Clynic.Domain.Models
{
    public class LandingPageConfig
    {
        public int Id { get; set; }
        public int IdClinica { get; set; }
        public string NombreLanding { get; set; } = string.Empty;
        public string HeroTitulo { get; set; } = string.Empty;
        public string HeroSubtitulo { get; set; } = string.Empty;
        public string DescripcionGeneral { get; set; } = string.Empty;
        public string TelefonoContacto { get; set; } = string.Empty;
        public string CorreoContacto { get; set; } = string.Empty;
        public string DireccionContacto { get; set; } = string.Empty;
        public string WhatsappContacto { get; set; } = string.Empty;
        public string CtaPrincipalTexto { get; set; } = string.Empty;
        public string CtaPrincipalUrl { get; set; } = string.Empty;
        public string ServiciosJson { get; set; } = "[]";
        public string MetaTitulo { get; set; } = string.Empty;
        public string MetaDescripcion { get; set; } = string.Empty;
        public string DominioBase { get; set; } = string.Empty;
        public bool MostrarHorariosSucursal { get; set; } = true;
        public bool Publicada { get; set; } = false;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        public Clinica? Clinica { get; set; }
    }
}
