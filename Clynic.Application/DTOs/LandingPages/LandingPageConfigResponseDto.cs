namespace Clynic.Application.DTOs.LandingPages
{
    public class LandingPageConfigResponseDto
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
        public List<LandingServiceItemDto> ServiciosDestacados { get; set; } = new();
        public string MetaTitulo { get; set; } = string.Empty;
        public string MetaDescripcion { get; set; } = string.Empty;
        public string DominioBase { get; set; } = string.Empty;
        public bool MostrarHorariosSucursal { get; set; }
        public bool Publicada { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
