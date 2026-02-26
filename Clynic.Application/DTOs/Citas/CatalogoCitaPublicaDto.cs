namespace Clynic.Application.DTOs.Citas
{
    public class CatalogoCitaPublicaDto
    {
        public int IdClinica { get; set; }
        public IReadOnlyCollection<CatalogoSucursalDto> Sucursales { get; set; } = Array.Empty<CatalogoSucursalDto>();
        public IReadOnlyCollection<CatalogoServicioDto> Servicios { get; set; } = Array.Empty<CatalogoServicioDto>();
    }

    public class CatalogoSucursalDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
    }

    public class CatalogoServicioDto
    {
        public int Id { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public int DuracionMin { get; set; }
        public decimal PrecioBase { get; set; }
    }
}
