using Clynic.Application.Interfaces.Repositories;

namespace Clynic.Application.Rules
{
    public class ServicioRules
    {
        private readonly IServicioRepository _repository;

        public ServicioRules(IServicioRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public bool NombreEsValido(string nombre, int longitudMinima = 3)
        {
            return !string.IsNullOrWhiteSpace(nombre) && nombre.Trim().Length >= longitudMinima;
        }

        public bool DuracionEsValida(int duracionMin)
        {
            return duracionMin > 0 && duracionMin <= 600;
        }

        public bool PrecioEsValido(decimal precio)
        {
            return precio >= 0;
        }

        public async Task<bool> NombreEsUnicoAsync(string nombreServicio, int idClinica, int? idExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(nombreServicio) || idClinica <= 0)
                return false;

            var existe = await _repository.ExisteNombreAsync(nombreServicio.Trim(), idClinica, idExcluir);
            return !existe;
        }

        public async Task<bool> ServicioExisteAsync(int id)
        {
            return await _repository.ExisteAsync(id);
        }
    }
}
