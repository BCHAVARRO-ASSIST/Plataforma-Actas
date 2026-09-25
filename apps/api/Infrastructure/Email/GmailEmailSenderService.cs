using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PlataformaActas.Api.Modules.Integrations.Google;

namespace PlataformaActas.Api.Infrastructure.Email;

/// <summary>
/// Servicio nativo de despacho de notificaciones por correo electrónico a través de la API oficial de Google Workspace (Gmail v1).
/// Utiliza delegación de autoridad a nivel de dominio (Domain-Wide Delegation) con una cuenta de servicio autorizada,
/// impersonando institucionalmente al buzón oficial soporte@assist.com.co sin depender del protocolo SMTP.
/// </summary>
public sealed class GmailEmailSenderService : IEmailSenderService
{
    public const string OfficialSenderEmail = "soporte@assist.com.co";
    public const string OfficialSenderName = "Soporte - Assist Consultores";
    public const string GmailSendScope = "https://www.googleapis.com/auth/gmail.send";

    private readonly GoogleIntegrationOptions _googleOptions;
    private readonly SmtpOptions _smtpOptions;
    private readonly IHostEnvironment _env;
    private readonly ILogger<GmailEmailSenderService> _logger;

    public GmailEmailSenderService(
        IOptions<GoogleIntegrationOptions> googleOptions,
        IOptions<SmtpOptions> smtpOptions,
        IHostEnvironment env,
        ILogger<GmailEmailSenderService> logger)
    {
        _googleOptions = googleOptions.Value;
        _smtpOptions = smtpOptions.Value;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Construye una estructura de mensaje RFC 2822 (MimeMessage) con cabeceras institucionales seguras.
    /// </summary>
    public static MimeMessage CreateMimeMessage(
        ActaEmailNotification notification,
        string? senderEmail = null,
        string? senderName = null)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var resolvedSenderEmail = !string.IsNullOrWhiteSpace(senderEmail) ? senderEmail.Trim() : OfficialSenderEmail;
        var resolvedSenderName = !string.IsNullOrWhiteSpace(senderName) ? senderName.Trim() : OfficialSenderName;

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

    /// <summary>
    /// Despacha la notificación del acta a través de la API oficial de Gmail utilizando Domain-Wide Delegation.
    /// </summary>
    public async Task SendActaNotificationAsync(ActaEmailNotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var senderEmail = !string.IsNullOrWhiteSpace(_googleOptions.GmailImpersonationEmail)
            ? _googleOptions.GmailImpersonationEmail
            : (!string.IsNullOrWhiteSpace(_smtpOptions.SenderEmail) ? _smtpOptions.SenderEmail : OfficialSenderEmail);

        var senderName = !string.IsNullOrWhiteSpace(_smtpOptions.SenderName)
            ? _smtpOptions.SenderName
            : OfficialSenderName;

        var message = CreateMimeMessage(notification, senderEmail, senderName);

        if (message.To.Count == 0)
        {
            _logger.LogWarning("No se especificaron destinatarios principales válidos para el acta {NumeroActa}. Omitiendo envío de correo.", notification.NumeroActa);
            return;
        }

        var keyPath = ResolveKeyPath(_googleOptions.ServiceAccountKeyPath);
        if (string.IsNullOrWhiteSpace(keyPath) || !File.Exists(keyPath))
        {
            _logger.LogError("No se encontró el archivo de credenciales de Google Service Account en: {KeyPath}. Omitiendo envío del acta {NumeroActa}.", keyPath, notification.NumeroActa);
            return;
        }

        try
        {
            var impersonationEmail = !string.IsNullOrWhiteSpace(_googleOptions.GmailImpersonationEmail)
                ? _googleOptions.GmailImpersonationEmail
                : (!string.IsNullOrWhiteSpace(_googleOptions.AdminImpersonationEmail) ? _googleOptions.AdminImpersonationEmail : OfficialSenderEmail);

            var credential = GoogleCredential.FromFile(keyPath)
                .CreateScoped(GmailSendScope)
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
                .TrimEnd('=');

            var gmailMessage = new Google.Apis.Gmail.v1.Data.Message
            {
                Raw = rawBase64Url
            };

            var sentMessage = await gmailService.Users.Messages.Send(gmailMessage, "me").ExecuteAsync(cancellationToken);

            _logger.LogInformation(
                "Notificación por correo del acta {NumeroActa} enviada exitosamente vía Google Workspace Gmail API (Id: {MessageId}) impersonando a {Impersonation} (Para: {ToCount}, Cc: {CcCount}, Bcc: {BccCount}).",
                notification.NumeroActa,
                sentMessage.Id,
                impersonationEmail,
                message.To.Count,
                message.Cc.Count,
                message.Bcc.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al enviar notificación por correo del acta {NumeroActa} mediante Google Gmail API.", notification.NumeroActa);
        }
    }

    private string? ResolveKeyPath(string? path)
    {
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(path))
        {
            if (Path.IsPathRooted(path))
            {
                candidates.Add(path);
            }
            else
            {
                candidates.Add(Path.Combine(_env.ContentRootPath, path));
                candidates.Add(Path.Combine(AppContext.BaseDirectory, path));
            }
        }

        // Ubicaciones estándar de credenciales
        candidates.Add(Path.Combine(_env.ContentRootPath, "Secrets", "google-credentials.json"));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, "Secrets", "google-credentials.json"));
        candidates.Add(Path.Combine(_env.ContentRootPath, "Resources", "google-credentials.json"));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, "Resources", "google-credentials.json"));

        return candidates.FirstOrDefault(File.Exists);
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
