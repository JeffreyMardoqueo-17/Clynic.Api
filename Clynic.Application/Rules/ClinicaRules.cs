using Clynic.Application.Interfaces.Repositories;

namespace Clynic.Application.Rules
{
    /// <summary>
    /// Clase con las reglas de negocio para las Clínicas
    /// </summary>
    public class ClinicaRules
    {
        private readonly IClinicaRepository _repository;

        public ClinicaRules(IClinicaRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Valida que el nombre de la clínica no esté duplicado
        /// </summary>
        public async Task<bool> NombreEsUnicoAsync(string nombre, int? idExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            var existe = await _repository.ExisteNombreAsync(nombre.Trim(), idExcluir);
            return !existe;
        }

        /// <summary>
        /// Valida que el nombre de la clínica tenga una longitud mínima
        /// </summary>
        public bool NombreTieneLongitudMinima(string nombre, int longitudMinima = 3)
        {
            return !string.IsNullOrWhiteSpace(nombre) && nombre.Trim().Length >= longitudMinima;
        }

        /// <summary>
        /// Valida que el teléfono tenga un formato básico válido
        /// </summary>
        public bool TelefonoEsValido(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return false;

            // Permitir solo dígitos, espacios, guiones y paréntesis
            var caracteresPermitidos = telefono.All(c => 
                char.IsDigit(c) || c == ' ' || c == '-' || c == '(' || c == ')' || c == '+');

            return caracteresPermitidos && telefono.Trim().Length >= 7;
        }

        /// <summary>
        /// Valida que la dirección tenga una longitud mínima
        /// </summary>
        public bool DireccionEsValida(string direccion, int longitudMinima = 5)
        {
            return !string.IsNullOrWhiteSpace(direccion) && direccion.Trim().Length >= longitudMinima;
        }

        /// <summary>
        /// Valida que la clínica exista en el sistema
        /// </summary>
        public async Task<bool> ClinicaExisteAsync(int id)
        {
            return await _repository.ExisteAsync(id);
        }
    }
}
