using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace PlataformaActas.Api.Modules.Auth.Services;

public sealed class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:SmtpHost"];
        var port = _configuration.GetValue<int>("Email:SmtpPort");
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var from = _configuration["Email:From"];
        var fromName = _configuration["Email:FromName"] ?? "Plataforma de Actas";
        var enableSsl = _configuration.GetValue<bool>("Email:EnableSsl");

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException("La configuración SMTP no está completa.");
        }

        using var message = new MailMessage();
        message.From = new MailAddress(from, fromName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = "Restablecimiento de contraseña - Plataforma de Actas";
        message.IsBodyHtml = true;

        message.Body = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <title>Restablecimiento de contraseña</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #f4f6f8; padding: 30px;">
                <div style="max-width: 600px; margin: auto; background: white; padding: 35px; border-radius: 12px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);">
                    <h2 style="color: #1e293b; margin-top: 0;">Restablecimiento de contraseña</h2>
                    <p style="color: #475569; font-size: 15px; line-height: 1.5;">
                        Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en la Plataforma de Actas.
                    </p>
                    <p style="color: #475569; font-size: 15px; line-height: 1.5;">
                        Haz clic en el siguiente botón para establecer una nueva contraseña:
                    </p>
                    <p style="text-align: center; margin: 30px 0;">
                        <a href="{WebUtility.HtmlEncode(resetLink)}"
                           style="display: inline-block; padding: 12px 24px; background: #2563eb; color: white; text-decoration: none; border-radius: 8px; font-weight: bold;">
                            Restablecer contraseña
                        </a>
                    </p>
                    <p style="color: #64748b; font-size: 13px;">
                        Este enlace será válido durante <strong>30 minutos</strong>.
                    </p>
                    <p style="color: #64748b; font-size: 13px;">
                        Si no solicitaste este cambio, puedes ignorar este mensaje con total seguridad.
                    </p>
                    <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 25px 0;">
                    <p style="font-size: 12px; color: #94a3b8; margin-bottom: 0;">
                        Plataforma de Actas — Seguridad de la Información
                    </p>
                </div>
            </body>
            </html>
            """;

        using var smtp = new SmtpClient(host, port);
        smtp.EnableSsl = enableSsl;
        smtp.Credentials = new NetworkCredential(username, password);

        await smtp.SendMailAsync(message, cancellationToken);
    }
}
