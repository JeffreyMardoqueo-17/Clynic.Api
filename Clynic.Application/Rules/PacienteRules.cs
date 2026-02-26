namespace Clynic.Application.Rules
{
    public class PacienteRules
    {
        public bool NombreValido(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor) && valor.Trim().Length >= 2;
        }

        public bool CorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                return false;
            }

            try
            {
                var _ = new System.Net.Mail.MailAddress(correo.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool FechaNacimientoValida(DateTime? fechaNacimiento)
        {
            if (!fechaNacimiento.HasValue)
            {
                return true;
            }

            return fechaNacimiento.Value.Date <= DateTime.UtcNow.Date;
        }
    }
}
