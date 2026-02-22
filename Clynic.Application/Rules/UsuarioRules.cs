using Clynic.Application.Interfaces.Repositories;

namespace Clynic.Application.Rules
{
    public class UsuarioRules
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IClinicaRepository _clinicaRepository;

        public UsuarioRules(
            IUsuarioRepository usuarioRepository,
            IClinicaRepository clinicaRepository)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
        }

        public async Task<bool> CorreoEsUnicoAsync(string correo, int? idExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            var existe = await _usuarioRepository.ExisteCorreoAsync(correo.Trim().ToLower(), idExcluir);
            return !existe;
        }

        public async Task<bool> ClinicaExisteAsync(int idClinica)
        {
            return await _clinicaRepository.ExisteAsync(idClinica);
        }

        public async Task<bool> UsuarioExisteAsync(int id)
        {
            return await _usuarioRepository.ExisteAsync(id);
        }

        public bool ClaveEsValida(string clave)
        {
            if (string.IsNullOrWhiteSpace(clave))
                return false;

            if (clave.Length < 6)
                return false;

            return true;
        }

        public bool CorreoTieneFormatoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo.Trim().ToLower();
            }
            catch
            {
                return false;
            }
        }
    }
}
