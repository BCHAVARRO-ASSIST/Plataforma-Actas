using System.Net;
using System.Net.Mail;

namespace PlataformaActas.Api.Features.Auth;

public sealed class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(
        string destinationEmail,
        string resetUrl)
    {
        var host =
            _configuration["Email:SmtpHost"];

        var port =
            _configuration.GetValue<int>(
                "Email:SmtpPort");

        var username =
            _configuration["Email:Username"];

        var password =
            _configuration["Email:Password"];

        var from =
            _configuration["Email:From"];

        var fromName =
            _configuration["Email:FromName"]
            ?? "Plataforma de Actas";

        var enableSsl =
            _configuration.GetValue<bool>(
                "Email:EnableSsl");

        if (
            string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException(
                "La configuración SMTP no está completa.");
        }

        using var message =
            new MailMessage();

        message.From =
            new MailAddress(
                from,
                fromName);

        message.To.Add(
            new MailAddress(
                destinationEmail));

        message.Subject =
            "Restablecimiento de contraseña - Plataforma de Actas";

        message.IsBodyHtml = true;

        message.Body = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <title>Restablecimiento de contraseña</title>
            </head>

            <body style="
                font-family: Arial, sans-serif;
                background-color: #f4f6f8;
                padding: 30px;
            ">

                <div style="
                    max-width: 600px;
                    margin: auto;
                    background: white;
                    padding: 35px;
                    border-radius: 12px;
                ">

                    <h2>
                        Restablecimiento de contraseña
                    </h2>

                    <p>
                        Hemos recibido una solicitud para
                        restablecer la contraseña de tu cuenta
                        en la Plataforma de Actas.
                    </p>

                    <p>
                        Haz clic en el siguiente botón para
                        establecer una nueva contraseña:
                    </p>

                    <p style="text-align:center;">
                        <a href="{WebUtility.HtmlEncode(resetUrl)}"
                           style="
                              display:inline-block;
                              padding:12px 24px;
                              background:#2563eb;
                              color:white;
                              text-decoration:none;
                              border-radius:8px;
                           ">
                            Restablecer contraseña
                        </a>
                    </p>

                    <p>
                        Este enlace será válido durante
                        <strong>30 minutos</strong>.
                    </p>

                    <p>
                        Si no solicitaste este cambio,
                        puedes ignorar este mensaje.
                    </p>

                    <hr>

                    <p style="
                        font-size:12px;
                        color:#6b7280;
                    ">
                        Plataforma de Actas
                    </p>

                </div>

            </body>
            </html>
            """;

        using var smtp =
            new SmtpClient(
                host,
                port);

        smtp.EnableSsl =
            enableSsl;

        smtp.Credentials =
            new NetworkCredential(
                username,
                password);

        await smtp.SendMailAsync(message);
    }
}