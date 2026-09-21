using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PlataformaActas.Api.Modules.Integrations.Google;

namespace PlataformaActas.Api.Infrastructure.Email;

public sealed class SmtpEmailSenderService : IEmailSenderService
{
    private readonly SmtpOptions _options;
    private readonly GoogleIntegrationOptions _googleOptions;
    private readonly ILogger<SmtpEmailSenderService> _logger;

    public SmtpEmailSenderService(
        IOptions<SmtpOptions> options,
        IOptions<GoogleIntegrationOptions> googleOptions,
        ILogger<SmtpEmailSenderService> logger)
    {
        _options = options.Value;
        _googleOptions = googleOptions.Value;
        _logger = logger;
    }

    public async Task SendActaNotificationAsync(ActaEmailNotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var senderEmail = !string.IsNullOrWhiteSpace(_options.FromEmail)
            ? _options.FromEmail
            : (!string.IsNullOrWhiteSpace(_googleOptions.AdminImpersonationEmail) ? _googleOptions.AdminImpersonationEmail : "soporte@assist.com.co");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, senderEmail));

        // 1. Destinatarios principales (Para)
        foreach (var email in notification.ToEmails.Where(IsValidEmail))
        {
            message.To.Add(MailboxAddress.Parse(email.Trim()));
        }

        if (message.To.Count == 0)
        {
            _logger.LogWarning("No se especificaron destinatarios principales válidos para el acta {NumeroActa}. Omitiendo envío.", notification.NumeroActa);
            return;
        }

        // 2. Con Copia (Cc)
        foreach (var email in notification.CcEmails.Where(IsValidEmail))
        {
            // Evitar duplicar si ya está en To
            if (!message.To.Mailboxes.Any(m => string.Equals(m.Address, email.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                message.Cc.Add(MailboxAddress.Parse(email.Trim()));
            }
        }

        // 3. Con Copia Oculta (Bcc)
        foreach (var email in notification.BccEmails.Where(IsValidEmail))
        {
            var trimmed = email.Trim();
            if (!message.To.Mailboxes.Any(m => string.Equals(m.Address, trimmed, StringComparison.OrdinalIgnoreCase)) &&
                !message.Cc.Mailboxes.Any(m => string.Equals(m.Address, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                message.Bcc.Add(MailboxAddress.Parse(trimmed));
            }
        }

        message.Subject = $"Acta Oficial Firmada - {notification.NumeroActa} ({notification.TipoActa}) - {notification.DestinatarioNombre}";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = BuildHtmlTemplate(notification)
        };

        if (notification.PdfBytes is { Length: > 0 })
        {
            bodyBuilder.Attachments.Add(
                $"Acta_{notification.NumeroActa}.pdf",
                notification.PdfBytes,
                new ContentType("application", "pdf"));
        }

        message.Body = bodyBuilder.ToMessageBody();

        // ---------------------------------------------------------------------------------
        // INTENTO 1: Google Workspace Gmail API oficial (Service Account con Domain-Wide Delegation)
        // ---------------------------------------------------------------------------------
        if (_googleOptions.Enabled && !string.IsNullOrWhiteSpace(_googleOptions.ServiceAccountKeyPath))
        {
            var keyPath = Path.IsPathRooted(_googleOptions.ServiceAccountKeyPath)
                ? _googleOptions.ServiceAccountKeyPath
                : Path.Combine(AppContext.BaseDirectory, _googleOptions.ServiceAccountKeyPath);

            if (File.Exists(keyPath))
            {
                try
                {
                    var impersonationEmail = !string.IsNullOrWhiteSpace(_googleOptions.AdminImpersonationEmail)
                        ? _googleOptions.AdminImpersonationEmail
                        : senderEmail;

                    var credential = GoogleCredential.FromFile(keyPath)
                        .CreateScoped(GmailService.ScopeConstants.GmailSend)
                        .CreateWithUser(impersonationEmail);

                    using var gmailService = new GmailService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "PlataformaActas"
                    });

                    using var ms = new MemoryStream();
                    await message.WriteToAsync(ms, cancellationToken);
                    var rawBase64Url = Convert.ToBase64String(ms.ToArray())
                        .Replace('+', '-')
                        .Replace('/', '_')
                        .Replace("=", "");

                    var gmailMessage = new Google.Apis.Gmail.v1.Data.Message
                    {
                        Raw = rawBase64Url
                    };

                    await gmailService.Users.Messages.Send(gmailMessage, "me").ExecuteAsync(cancellationToken);

                    _logger.LogInformation(
                        "Notificación por correo del acta {NumeroActa} enviada exitosamente vía Google Workspace Gmail API impersonando a {Impersonation} (Para: {ToCount}, Cc: {CcCount}, Bcc: {BccCount}).",
                        notification.NumeroActa,
                        impersonationEmail,
                        message.To.Count,
                        message.Cc.Count,
                        message.Bcc.Count);

                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Envío por Gmail API no completado para el acta {NumeroActa}. Intentando fallback vía SMTP tradicional.", notification.NumeroActa);
                }
            }
        }

        // ---------------------------------------------------------------------------------
        // INTENTO 2: Fallback vía Servidor SMTP tradicional (MailKit)
        // ---------------------------------------------------------------------------------
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogWarning("Configuración SMTP no definida. Omitiendo envío de correo para el acta {NumeroActa}.", notification.NumeroActa);
            return;
        }

        try
        {
            using var client = new SmtpClient();

            // Configuración de certificados / SSL
            var secureSocketOptions = _options.EnableSsl
                ? (_options.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable)
                : SecureSocketOptions.None;

            await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.UserName) && !string.IsNullOrWhiteSpace(_options.Password))
            {
                await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Notificación por correo del acta {NumeroActa} enviada exitosamente vía SMTP a {ToCount} destinatarios (Cc: {CcCount}, Bcc: {BccCount}).",
                notification.NumeroActa,
                message.To.Count,
                message.Cc.Count,
                message.Bcc.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación por correo del acta {NumeroActa} vía SMTP ({Host}:{Port}).",
                notification.NumeroActa, _options.Host, _options.Port);
        }
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email.Trim());
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    public static string BuildHtmlTemplate(ActaEmailNotification notification)
    {
        string fechaFirmaStr;
        try
        {
            var tzId = OperatingSystem.IsWindows() ? "SA Pacific Standard Time" : "America/Bogota";
            var tzColombia = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var utcTime = DateTime.SpecifyKind(notification.FechaFirmaUtc ?? DateTime.UtcNow, DateTimeKind.Utc);
            var fechaLocal = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tzColombia);
            fechaFirmaStr = fechaLocal.ToString("dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            var utcTime = DateTime.SpecifyKind(notification.FechaFirmaUtc ?? DateTime.UtcNow, DateTimeKind.Utc);
            fechaFirmaStr = utcTime.AddHours(-5).ToString("dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        }

        return $$"""
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <style>
            body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f6f8; color: #1e293b; }
            .container { max-width: 600px; margin: 24px auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); border: 1px solid #e2e8f0; }
            .header { background-color: #1e5ba8; padding: 24px 32px; text-align: left; color: #ffffff; }
            .header h1 { margin: 0; font-size: 20px; font-weight: 700; letter-spacing: -0.025em; }
            .header p { margin: 4px 0 0 0; font-size: 13px; opacity: 0.9; }
            .content { padding: 32px; }
            .greeting { font-size: 16px; font-weight: 600; margin-bottom: 12px; }
            .intro { font-size: 14px; line-height: 1.6; color: #475569; margin-bottom: 24px; }
            .details-table { width: 100%; border-collapse: collapse; margin-bottom: 8px; font-size: 13px; background-color: #f8fafc; border-radius: 6px; overflow: hidden; border: 1px solid #e2e8f0; }
            .details-table td { padding: 10px 14px; border-bottom: 1px solid #e2e8f0; }
            .details-table tr:last-child td { border-bottom: none; }
            .details-label { font-weight: 600; color: #64748b; width: 38%; }
            .details-value { color: #0f172a; font-weight: 500; }
            .footer { background-color: #f8fafc; padding: 20px 32px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
            .footer p { margin: 4px 0; }
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header">
              <h1>Assist Consultores de Sistemas S.A.</h1>
              <p>Plataforma de Gestión de Actas y Activos TI</p>
            </div>
            <div class="content">
              <div class="greeting">Estimado(a) {{System.Net.WebUtility.HtmlEncode(notification.DestinatarioNombre)}},</div>
              <p class="intro">
                Le informamos que se ha completado y firmado formalmente el <strong>Acta de {{System.Net.WebUtility.HtmlEncode(notification.TipoActa)}}</strong> con número oficial <strong>{{System.Net.WebUtility.HtmlEncode(notification.NumeroActa)}}</strong>.
              </p>
              
              <table class="details-table">
                <tr>
                  <td class="details-label">Número de Acta:</td>
                  <td class="details-value"><strong>{{System.Net.WebUtility.HtmlEncode(notification.NumeroActa)}}</strong></td>
                </tr>
                <tr>
                  <td class="details-label">Tipo de Movimiento:</td>
                  <td class="details-value">{{System.Net.WebUtility.HtmlEncode(notification.TipoActa)}}</td>
                </tr>
                <tr>
                  <td class="details-label">Colaborador Receptor:</td>
                  <td class="details-value">{{System.Net.WebUtility.HtmlEncode(notification.DestinatarioNombre)}}</td>
                </tr>
                <tr>
                  <td class="details-label">Activo Tecnológico:</td>
                  <td class="details-value">{{System.Net.WebUtility.HtmlEncode(notification.EquipoMarca)}} (Serial / Placa: {{System.Net.WebUtility.HtmlEncode(notification.EquipoSerial)}})</td>
                </tr>
                <tr>
                  <td class="details-label">Fecha y Hora de Firma:</td>
                  <td class="details-value">{{fechaFirmaStr}}</td>
                </tr>
              </table>
            </div>
            <div class="footer">
              <p><strong>Assist Consultores de Sistemas S.A.</strong> &bull; Soporte de Tecnología e Infraestructura</p>
              <p>Este es un mensaje generado automáticamente por la Plataforma de Actas. Por favor no responda a este correo.</p>
            </div>
          </div>
        </body>
        </html>
        """;
    }
}
