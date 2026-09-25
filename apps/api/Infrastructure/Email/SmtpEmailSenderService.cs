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

    public static MimeMessage CreateMimeMessage(
        ActaEmailNotification notification,
        string senderEmail,
        string senderName)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var resolvedSenderEmail = !string.IsNullOrWhiteSpace(senderEmail) ? senderEmail.Trim() : "soporte@assist.com.co";
        var resolvedSenderName = !string.IsNullOrWhiteSpace(senderName) ? senderName.Trim() : "Soporte - Assist Consultores";

        var message = new MimeMessage();
        var mailbox = new MailboxAddress(resolvedSenderName, resolvedSenderEmail);
        message.From.Add(mailbox);
        message.ReplyTo.Add(mailbox);

        // 1. Destinatarios principales (Para)
        foreach (var email in notification.ToEmails.Where(IsValidEmail))
        {
            message.To.Add(MailboxAddress.Parse(email.Trim()));
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
        return message;
    }

    public async Task SendActaNotificationAsync(ActaEmailNotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Remitente genérico institucional: estrictamente SenderEmail / FromEmail y SenderName / FromName
        // NUNCA reutilizar UserName (credencial de autenticación) como remitente
        var senderEmail = !string.IsNullOrWhiteSpace(_options.SenderEmail)
            ? _options.SenderEmail
            : (!string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.FromEmail : "soporte@assist.com.co");

        var senderName = !string.IsNullOrWhiteSpace(_options.SenderName)
            ? _options.SenderName
            : (!string.IsNullOrWhiteSpace(_options.FromName) ? _options.FromName : "Soporte - Assist Consultores");

        var message = CreateMimeMessage(notification, senderEmail, senderName);

        if (message.To.Count == 0)
        {
            _logger.LogWarning("No se especificaron destinatarios principales válidos para el acta {NumeroActa}. Omitiendo envío.", notification.NumeroActa);
            return;
        }

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
        return EmailTemplateBuilder.BuildActaNotificationHtml(notification);
    }
}
