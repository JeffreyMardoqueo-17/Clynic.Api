namespace Clynic.Application.DTOs.LandingPages
{
    public class LandingPublicResponseDto
    {
        public int IdClinica { get; set; }
        public int IdSucursal { get; set; }
        public string SlugClinica { get; set; } = string.Empty;
        public string NombreClinica { get; set; } = string.Empty;
        public string NombreSucursal { get; set; } = string.Empty;
        public string SubdominioSucursal { get; set; } = string.Empty;
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
        public List<LandingServicioClinicaDto> ServiciosClinica { get; set; } = new();
        public List<LandingHorarioDto> HorariosSucursal { get; set; } = new();
        public string MetaTitulo { get; set; } = string.Empty;
        public string MetaDescripcion { get; set; } = string.Empty;
    }
}
