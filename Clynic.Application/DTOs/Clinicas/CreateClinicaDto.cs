namespace Clynic.Application.DTOs.Clinicas
{
    /// <summary>
    /// DTO para la creación de una nueva clínica
    /// </summary>
    public class CreateClinicaDto
    {
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
    }
}
