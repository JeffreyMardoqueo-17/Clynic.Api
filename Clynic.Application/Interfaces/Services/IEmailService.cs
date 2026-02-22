namespace Clynic.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task EnviarCodigoVerificacionAsync(string emailDestino, string nombreUsuario, string codigo);
        Task EnviarEmailAsync(string emailDestino, string asunto, string cuerpoHtml);
    }
}
