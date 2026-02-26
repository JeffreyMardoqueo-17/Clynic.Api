namespace Clynic.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task EnviarCodigoVerificacionAsync(string emailDestino, string nombreUsuario, string codigo);
        Task EnviarBienvenidaUsuarioAsync(string emailDestino, string nombreUsuario, string nombreClinica, string rol);
        Task EnviarCredencialesTemporalesAsync(string emailDestino, string nombreUsuario, string nombreClinica, string rol, string claveTemporal);
        Task EnviarEmailAsync(string emailDestino, string asunto, string cuerpoHtml);
    }
}
