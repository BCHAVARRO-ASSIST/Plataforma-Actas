using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:SmtpHost"];
        if (string.IsNullOrWhiteSpace(host))
        {
            return;
        }

        var port = int.TryParse(configuration["Email:SmtpPort"], out var p) ? p : 587;
        var username = configuration["Email:Username"];
        var password = configuration["Email:Password"];
        var from = configuration["Email:From"] ?? "noreply@plataformaactas.local";
        var fromName = configuration["Email:FromName"] ?? "Plataforma de Actas";
        var enableSsl = bool.TryParse(configuration["Email:EnableSsl"], out var ssl) && ssl;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = !string.IsNullOrWhiteSpace(username)
                ? new NetworkCredential(username, password)
                : null
        };

        var message = new MailMessage
        {
            From = new MailAddress(from, fromName),
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

        message.To.Add(toEmail);

        await client.SendMailAsync(message, cancellationToken);
    }
}
