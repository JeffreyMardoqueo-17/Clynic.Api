namespace Clynic.Application.DTOs.Sucursales
{
    /// <summary>
    /// DTO de respuesta con la informacion de una sucursal
    /// </summary>
    public class SucursalResponseDto
    {
        /// <summary>
        /// ID de la sucursal
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la clinica
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

        /// <summary>
        /// Indica si la sucursal esta activa
        /// </summary>
        public bool Activa { get; set; }
    }
}
