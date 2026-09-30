using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Modules.Auth.Data.Entities;
using PlataformaActas.Api.Modules.Auth.Models.Dto;

namespace PlataformaActas.Api.Modules.Auth.Services;

public sealed class AuthService(PlataformaActasDbContext context) : IAuthService
{

    public async Task<LoginData?> AuthenticateAsync(
        string usuario,
        string password,
        CancellationToken cancellationToken = default)
    {
        var trimmed = usuario.Trim().ToLower();

        var user = await context.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                    .ThenInclude(r => r.RolPermisos)
                        .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(
                u => u.UsuarioRed.ToLower() == trimmed || u.Correo.ToLower() == trimmed,
                cancellationToken);

        if (user is null || !user.Activo)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isPasswordValid && !string.IsNullOrEmpty(password))
        {
            var altCapitalized = char.IsUpper(password[0])
                ? char.ToLower(password[0]) + (password.Length > 1 ? password.Substring(1) : "")
                : char.ToUpper(password[0]) + (password.Length > 1 ? password.Substring(1) : "");

            if (BCrypt.Net.BCrypt.Verify(altCapitalized, user.PasswordHash))
            {
                isPasswordValid = true;
            }
        }

        if (!isPasswordValid)
        {
            return null;
        }

        var rolActivo = user.UsuarioRoles
            .Select(ur => ur.Rol)
            .FirstOrDefault(r => r != null && r.Activo);

        var usuarioData = new UsuarioAuthResponse(
            user.Id,
            user.Nombre,
            user.Apellido,
            user.UsuarioRed,
            user.Correo,
            user.Proceso
        );

        var rolData = new RolAuthResponse(
            rolActivo?.Id ?? 0,
            rolActivo?.Nombre ?? "Sin rol",
            rolActivo?.Descripcion ?? string.Empty
        );

        var permisos = rolActivo?.RolPermisos
            .Where(rp => rp.Permiso != null && rp.Permiso.Activo)
            .Select(rp => rp.Permiso.Nombre)
            .Distinct()
            .OrderBy(p => p)
            .ToList() ?? new List<string>();

        return new LoginData(usuarioData, rolData, permisos);
    }
}

public interface IPasswordResetService
{
    Task<PasswordResetRequestResult> CreateResetRequestAsync(
        string identifier,
        CancellationToken cancellationToken = default);

    Task<PasswordResetResult> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);
}

public sealed class PasswordResetService(PlataformaActasDbContext context) : IPasswordResetService
{
    public async Task<PasswordResetRequestResult> CreateResetRequestAsync(
        string identifier,
        CancellationToken cancellationToken = default)
    {
        var trimmed = identifier.Trim().ToLower();

        var user = await context.Usuarios
            .FirstOrDefaultAsync(
                u => u.UsuarioRed.ToLower() == trimmed || u.Correo.ToLower() == trimmed,
                cancellationToken);

        if (user is null || !user.Activo)
        {
            return PasswordResetRequestResult.NotFound();
        }

        var rawToken = GenerateSecureToken();
        var tokenHash = HashToken(rawToken);
        var expiration = DateTime.UtcNow.AddMinutes(30);

        var resetToken = new PasswordResetToken
        {
            UsuarioId = user.Id,
            TokenHash = tokenHash,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = expiration
        };

        context.PasswordResetTokens.Add(resetToken);
        await context.SaveChangesAsync(cancellationToken);

        return PasswordResetRequestResult.CreatedSuccessfully(user.Correo, rawToken, expiration);
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(token);

        var record = await context.PasswordResetTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (record is null)
        {
            return PasswordResetResult.Invalid();
        }

        if (record.FechaUso != null || record.FechaExpiracion < DateTime.UtcNow)
        {
            return PasswordResetResult.Expired();
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        record.Usuario.PasswordHash = newPasswordHash;
        record.FechaUso = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return PasswordResetResult.Completed();
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes).ToLower();
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default);
}

public sealed class EmailService(IConfiguration configuration) : IEmailService
{
    public static MailMessage CreatePasswordResetMessage(
        string toEmail,
        string resetUrl,
        string senderEmail,
        string senderName)
    {
        var resolvedSenderEmail = !string.IsNullOrWhiteSpace(senderEmail) ? senderEmail.Trim() : "soporte@assist.com.co";
        var resolvedSenderName = !string.IsNullOrWhiteSpace(senderName) ? senderName.Trim() : "Soporte - Assist Consultores";

        var mailAddress = new MailAddress(resolvedSenderEmail, resolvedSenderName);
        var message = new MailMessage
        {
            From = mailAddress,
            Subject = "Recuperación de Contraseña - Plataforma de Actas",
            Body = $@"
                <h2>Restablecimiento de Contraseña</h2>
                <p>Has solicitado restablecer tu contraseña en la Plataforma de Actas.</p>
                <p>Haz clic en el siguiente enlace para continuar:</p>
                <p><a href='{resetUrl}'>{resetUrl}</a></p>
                <p>Este enlace expirará en 30 minutos.</p>
                <p>Si no realizaste esta solicitud, puedes ignorar este mensaje.</p>",
            IsBodyHtml = true
        };

        message.ReplyToList.Add(mailAddress);
        message.To.Add(toEmail);
        return message;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var senderEmail = configuration["Smtp:From"]
            ?? configuration["Smtp:FromEmail"]
            ?? configuration["Smtp:SenderEmail"]
            ?? configuration["EmailSettings:SenderEmail"]
            ?? configuration["EmailSettings:FromEmail"]
            ?? configuration["Email:From"]
            ?? "soporte@assist.com.co";

        var senderName = configuration["Smtp:FromName"]
            ?? configuration["Smtp:SenderName"]
            ?? configuration["EmailSettings:SenderName"]
            ?? configuration["EmailSettings:FromName"]
            ?? configuration["Email:FromName"]
            ?? "Soporte - Assist Consultores";

        // ---------------------------------------------------------------------------------
        // INTENTO 1: Google Workspace Gmail API oficial (Service Account con Domain-Wide Delegation)
        // ---------------------------------------------------------------------------------
        var keyPath = configuration["GoogleIntegration:ServiceAccountKeyPath"] ?? "Secrets/google-credentials.json";
        var resolvedKeyPath = keyPath;
        if (!Path.IsPathRooted(resolvedKeyPath))
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, resolvedKeyPath),
                Path.Combine(AppContext.BaseDirectory, "Secrets", "google-credentials.json"),
                Path.Combine(AppContext.BaseDirectory, "Resources", "google-credentials.json")
            };
            resolvedKeyPath = candidates.FirstOrDefault(File.Exists) ?? resolvedKeyPath;
        }

        if (File.Exists(resolvedKeyPath))
        {
            try
            {
                var impersonationEmail = configuration["GoogleIntegration:GmailImpersonationEmail"]
                    ?? configuration["GoogleIntegration:AdminImpersonationEmail"]
                    ?? "soporte@assist.com.co";

                var credential = GoogleCredential.FromFile(resolvedKeyPath)
                    .CreateScoped("https://www.googleapis.com/auth/gmail.send")
                    .CreateWithUser(impersonationEmail);

                using var gmailService = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "PlataformaActas"
                });

                var mimeMessage = new MimeMessage();
                var mailbox = new MailboxAddress(senderName, senderEmail);
                mimeMessage.From.Add(mailbox);
                mimeMessage.ReplyTo.Add(mailbox);
                mimeMessage.To.Add(MailboxAddress.Parse(toEmail));
                mimeMessage.Subject = "Recuperación de Contraseña - Plataforma de Actas";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <h2>Restablecimiento de Contraseña</h2>
                        <p>Has solicitado restablecer tu contraseña en la Plataforma de Actas.</p>
                        <p>Haz clic en el siguiente enlace para continuar:</p>
                        <p><a href='{resetUrl}'>{resetUrl}</a></p>
                        <p>Este enlace expirará en 30 minutos.</p>
                        <p>Si no realizaste esta solicitud, puedes ignorar este mensaje.</p>"
                };
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using var ms = new MemoryStream();
                await mimeMessage.WriteToAsync(ms, cancellationToken);
                var rawBase64Url = Convert.ToBase64String(ms.ToArray())
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .TrimEnd('=');

                var gmailMessage = new Google.Apis.Gmail.v1.Data.Message
                {
                    Raw = rawBase64Url
                };

                await gmailService.Users.Messages.Send(gmailMessage, "me").ExecuteAsync(cancellationToken);
                return;
            }
            catch
            {
                // Fallback transparente a SMTP tradicional si falla Google Workspace API
            }
        }

        // ---------------------------------------------------------------------------------
        // INTENTO 2: Fallback vía Servidor SMTP tradicional (System.Net.Mail)
        // ---------------------------------------------------------------------------------
        var host = configuration["Smtp:Host"]
            ?? configuration["Smtp:Server"]
            ?? configuration["EmailSettings:Server"]
            ?? configuration["EmailSettings:Host"]
            ?? configuration["Email:SmtpHost"];

        if (string.IsNullOrWhiteSpace(host))
        {
            return;
        }

        var portStr = configuration["Smtp:Port"]
            ?? configuration["EmailSettings:Port"]
            ?? configuration["Email:SmtpPort"];
        var port = int.TryParse(portStr, out var p) ? p : 587;

        var username = configuration["Smtp:User"]
            ?? configuration["Smtp:UserName"]
            ?? configuration["Smtp:Username"]
            ?? configuration["EmailSettings:Username"]
            ?? configuration["EmailSettings:User"]
            ?? configuration["Email:Username"];

        var password = configuration["Smtp:Password"]
            ?? configuration["EmailSettings:Password"]
            ?? configuration["Email:Password"];

        var enableSslStr = configuration["Smtp:EnableSsl"]
            ?? configuration["EmailSettings:EnableSsl"]
            ?? configuration["Email:EnableSsl"];
        var enableSsl = bool.TryParse(enableSslStr, out var ssl) ? ssl : true;

        using var client = new System.Net.Mail.SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = !string.IsNullOrWhiteSpace(username)
                ? new NetworkCredential(username, password)
                : null
        };

        var message = CreatePasswordResetMessage(toEmail, resetUrl, senderEmail, senderName);

        await client.SendMailAsync(message, cancellationToken);
    }
}
