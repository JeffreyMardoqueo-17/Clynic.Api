using Clynic.Application.Interfaces.Repositories;

namespace Clynic.Application.Rules
{
    /// <summary>
    /// Clase con las reglas de negocio para las Sucursales
    /// </summary>
    public class SucursalRules
    {
        private readonly ISucursalRepository _repository;

        public SucursalRules(ISucursalRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Valida que el nombre de la sucursal no este duplicado dentro de la clinica
        /// </summary>
        public async Task<bool> NombreEsUnicoAsync(string nombre, int idClinica, int? idExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(nombre) || idClinica <= 0)
                return false;

            var existe = await _repository.ExisteNombreAsync(nombre.Trim(), idClinica, idExcluir);
            return !existe;
        }

        /// <summary>
        /// Valida que el nombre de la sucursal tenga una longitud minima
        /// </summary>
        public bool NombreTieneLongitudMinima(string nombre, int longitudMinima = 3)
        {
            return !string.IsNullOrWhiteSpace(nombre) && nombre.Trim().Length >= longitudMinima;
        }

        /// <summary>
        /// Valida que la direccion tenga una longitud minima
        /// </summary>
        public bool DireccionEsValida(string direccion, int longitudMinima = 5)
        {
            return !string.IsNullOrWhiteSpace(direccion) && direccion.Trim().Length >= longitudMinima;
        }

        /// <summary>
        /// Valida que la sucursal exista en el sistema
        /// </summary>
        public async Task<bool> SucursalExisteAsync(int id)
        {
            return await _repository.ExisteAsync(id);
        }
    }
}
