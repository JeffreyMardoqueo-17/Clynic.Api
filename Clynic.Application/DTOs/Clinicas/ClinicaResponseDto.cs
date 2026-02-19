namespace Clynic.Application.DTOs.Clinicas
{
    /// <summary>
    /// DTO de respuesta con la información de una clínica
    /// </summary>
    public class ClinicaResponseDto
    {
        /// <summary>
        /// ID de la clínica
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la clínica
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono de contacto de la clínica
        /// </summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Dirección física de la clínica
        /// </summary>
        public string Direccion { get; set; } = string.Empty;

        /// <summary>
        /// Indica si la clínica está activa
        /// </summary>
        public bool Activa { get; set; }

        /// <summary>
        /// Fecha de creación de la clínica
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }
}
