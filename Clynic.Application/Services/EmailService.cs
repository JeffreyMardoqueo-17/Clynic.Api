using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Clynic.Application.Interfaces.Services;

namespace Clynic.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string? _emailSender;
        private readonly string? _emailPassword;
        private readonly string? _smtpHost;
        private readonly int _smtpPort;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailSender = _configuration["Email:Sender"];
            _emailPassword = _configuration["Email:Password"];
            _smtpHost = _configuration["Email:Host"];
            _smtpPort = _configuration.GetValue<int>("Email:Port", 587);
        }

        private SmtpClient CrearClienteSmtp()
        {
            return new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSender, _emailPassword)
            };
        }

        public async Task EnviarCodigoVerificacionAsync(string emailDestino, string nombreUsuario, string codigo)
        {
            var asunto = "Clynic - Código de Verificación para Cambio de Contraseña";
            var cuerpoHtml = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
        
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #007acc, #00a8e8); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>Clynic</h1>
            <p style='color: #e0f7ff; margin: 10px 0 0 0; font-size: 14px;'>Sistema de Gestión de Citas</p>
        </div>
        
        <!-- Body -->
        <div style='padding: 40px 30px;'>
            <h2 style='color: #007acc; margin-top: 0;'>Hola {nombreUsuario},</h2>
            
            <p style='font-size: 16px; line-height: 1.6; color: #555;'>
                Has solicitado cambiar tu contraseña. Utiliza el siguiente código de verificación para completar el proceso:
            </p>
            
            <!-- Código de verificación -->
            <div style='background-color: #f8f9fa; border: 2px dashed #007acc; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0;'>
                <p style='margin: 0 0 10px 0; color: #666; font-size: 14px;'>Tu código de verificación es:</p>
                <span style='font-size: 32px; font-weight: bold; color: #007acc; letter-spacing: 5px; font-family: monospace;'>{codigo}</span>
            </div>
            
            <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px;'>
                <p style='margin: 0; color: #856404; font-size: 14px;'>
                    <strong>⚠️ Importante:</strong> Este código expirará en <strong>15 minutos</strong> por seguridad.
                </p>
            </div>
            
            <p style='font-size: 14px; color: #777; line-height: 1.6;'>
                Si no solicitaste este cambio de contraseña, puedes ignorar este correo de forma segura. Tu cuenta permanecerá sin cambios.
            </p>
        </div>
        
        <!-- Footer -->
        <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
            <p style='margin: 0; color: #6c757d; font-size: 12px;'>
                Este es un correo automático, por favor no responda a este mensaje.
            </p>
            <p style='margin: 10px 0 0 0; color: #6c757d; font-size: 12px;'>
                © {DateTime.UtcNow.Year} Clynic - Sistema de Gestión de Citas
            </p>
        </div>
    </div>
</body>
</html>";

            await EnviarEmailHtmlAsync(emailDestino, asunto, cuerpoHtml);
        }

        public async Task EnviarBienvenidaUsuarioAsync(string emailDestino, string nombreUsuario, string nombreClinica, string rol)
        {
            var asunto = "Clynic - Bienvenido al sistema";
            var cuerpoHtml = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
        <div style='background: linear-gradient(135deg, #007acc, #00a8e8); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>Clynic</h1>
            <p style='color: #e0f7ff; margin: 10px 0 0 0; font-size: 14px;'>Sistema de Gestión de Citas</p>
        </div>

        <div style='padding: 40px 30px;'>
            <h2 style='color: #007acc; margin-top: 0;'>¡Bienvenido/a, {nombreUsuario}!</h2>

            <p style='font-size: 16px; line-height: 1.6; color: #555;'>
                Tu usuario ha sido registrado exitosamente en el sistema.
            </p>

            <div style='background-color: #f8f9fa; border: 1px solid #e9ecef; border-radius: 8px; padding: 18px; margin: 24px 0;'>
                <p style='margin: 0 0 8px 0; color: #495057; font-size: 14px;'><strong>Clínica:</strong> {nombreClinica}</p>
                <p style='margin: 0; color: #495057; font-size: 14px;'><strong>Rol:</strong> {rol}</p>
            </div>

            <p style='font-size: 14px; color: #777; line-height: 1.6;'>
                Si no reconoces este registro, por favor contacta al administrador de tu clínica.
            </p>
        </div>

        <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
            <p style='margin: 0; color: #6c757d; font-size: 12px;'>
                Este es un correo automático, por favor no responda a este mensaje.
            </p>
            <p style='margin: 10px 0 0 0; color: #6c757d; font-size: 12px;'>
                © {DateTime.UtcNow.Year} Clynic - Sistema de Gestión de Citas
            </p>
        </div>
    </div>
</body>
</html>";

            await EnviarEmailHtmlAsync(emailDestino, asunto, cuerpoHtml);
        }

        public async Task EnviarCredencialesTemporalesAsync(string emailDestino, string nombreUsuario, string nombreClinica, string rol, string claveTemporal)
        {
            var asunto = "Clynic - Credenciales de acceso temporal";
            var cuerpoHtml = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; color: #333; background-color: #f4f4f4; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);'>
        <div style='background: linear-gradient(135deg, #007acc, #00a8e8); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>Clynic</h1>
            <p style='color: #e0f7ff; margin: 10px 0 0 0; font-size: 14px;'>Sistema de Gestión de Citas</p>
        </div>

        <div style='padding: 40px 30px;'>
            <h2 style='color: #007acc; margin-top: 0;'>Hola {nombreUsuario},</h2>

            <p style='font-size: 16px; line-height: 1.6; color: #555;'>
                Se ha creado tu usuario en Clynic con acceso temporal.
            </p>

            <div style='background-color: #f8f9fa; border: 1px solid #e9ecef; border-radius: 8px; padding: 18px; margin: 24px 0;'>
                <p style='margin: 0 0 8px 0; color: #495057; font-size: 14px;'><strong>Clínica:</strong> {nombreClinica}</p>
                <p style='margin: 0 0 8px 0; color: #495057; font-size: 14px;'><strong>Rol:</strong> {rol}</p>
                <p style='margin: 0 0 8px 0; color: #495057; font-size: 14px;'><strong>Correo:</strong> {emailDestino}</p>
                <p style='margin: 0; color: #495057; font-size: 14px;'><strong>Contraseña temporal:</strong> <span style='font-family: monospace; font-size: 16px;'>{claveTemporal}</span></p>
            </div>

            <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px;'>
                <p style='margin: 0; color: #856404; font-size: 14px;'>
                    <strong>⚠️ Importante:</strong> Por seguridad, debes cambiar esta contraseña al iniciar sesión.
                </p>
            </div>
        </div>

        <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
            <p style='margin: 0; color: #6c757d; font-size: 12px;'>
                Este es un correo automático, por favor no responda a este mensaje.
            </p>
            <p style='margin: 10px 0 0 0; color: #6c757d; font-size: 12px;'>
                © {DateTime.UtcNow.Year} Clynic - Sistema de Gestión de Citas
            </p>
        </div>
    </div>
</body>
</html>";

            await EnviarEmailHtmlAsync(emailDestino, asunto, cuerpoHtml);
        }

        public async Task EnviarEmailAsync(string emailDestino, string asunto, string cuerpoHtml)
        {
            await EnviarEmailHtmlAsync(emailDestino, asunto, cuerpoHtml);
        }

        private async Task EnviarEmailHtmlAsync(string emailDestino, string asunto, string cuerpoHtml)
        {
            if (string.IsNullOrWhiteSpace(_emailSender) || string.IsNullOrWhiteSpace(_emailPassword))
            {
                throw new InvalidOperationException("La configuración de email no está completa. Verifica Email:Sender y Email:Password en la configuración.");
            }

            using var cliente = CrearClienteSmtp();
            using var mensaje = new MailMessage()
            {
                From = new MailAddress(_emailSender, "Clynic"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mensaje.To.Add(emailDestino);

            await cliente.SendMailAsync(mensaje);
        }
    }
}
