namespace Clynic.Application.DTOs.Sucursales
{
    /// <summary>
    /// DTO para la creacion de una nueva sucursal
    /// </summary>
    public class CreateSucursalDto
    {
        /// <summary>
        /// ID de la clinica a la que pertenece la sucursal
        /// </summary>
        public int IdClinica { get; set; }

        /// <summary>
        /// Nombre de la sucursal
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Direccion fisica de la sucursal
        /// </summary>
        public string Direccion { get; set; } = string.Empty;
    }
}
