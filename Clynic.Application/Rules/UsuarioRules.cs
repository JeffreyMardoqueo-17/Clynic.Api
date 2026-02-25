using Clynic.Application.Interfaces.Repositories;

namespace Clynic.Application.Rules
{
    public class UsuarioRules
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IClinicaRepository _clinicaRepository;
        private readonly ISucursalRepository _sucursalRepository;

        public UsuarioRules(
            IUsuarioRepository usuarioRepository,
            IClinicaRepository clinicaRepository,
            ISucursalRepository sucursalRepository)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _clinicaRepository = clinicaRepository ?? throw new ArgumentNullException(nameof(clinicaRepository));
            _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
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

        public async Task<bool> SucursalPerteneceAClinicaAsync(int idSucursal, int idClinica)
        {
            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(idSucursal);
            return sucursal != null && sucursal.IdClinica == idClinica;
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
