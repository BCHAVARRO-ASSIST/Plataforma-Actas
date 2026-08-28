using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Infrastructure.Security;
using PlataformaActas.Api.Modules.Auth.Data.Entities;
using PlataformaActas.Api.Modules.Auth.Models.Dto;

namespace PlataformaActas.Api.Modules.Auth.Services
{
    public interface IAuthService
    {
        Task<LoginData?> AuthenticateAsync(
            string usuario,
            string password,
            CancellationToken cancellationToken = default);
    }

    public sealed class AuthService : IAuthService
    {
        private readonly PlataformaActasDbContext _context;

        public AuthService(PlataformaActasDbContext context)
        {
            _context = context;
        }

        public async Task<LoginData?> AuthenticateAsync(
            string usuario,
            string password,
            CancellationToken cancellationToken = default)
        {
            var trimmed = usuario.Trim().ToLower();

            var user = await _context.Usuarios
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

    public interface IJwtTokenService
    {
        string GenerateToken(
            UsuarioAuthResponse usuario,
            RolAuthResponse? rol,
            IReadOnlyList<string> permisos);
    }

    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(
            UsuarioAuthResponse usuario,
            RolAuthResponse? rol,
            IReadOnlyList<string> permisos)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("No está configurada la clave JWT.");

            var issuer = _configuration["Jwt:Issuer"] ?? "PlataformaActas.Api";
            var audience = _configuration["Jwt:Audience"] ?? "PlataformaActas.Web";

            var expiresMinutes =
                int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var minutes)
                    ? minutes
                    : 60;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
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

    public sealed class PasswordResetService : IPasswordResetService
    {
        private readonly PlataformaActasDbContext _context;

        public PasswordResetService(PlataformaActasDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetRequestResult> CreateResetRequestAsync(
            string identifier,
            CancellationToken cancellationToken = default)
        {
            var trimmed = identifier.Trim().ToLower();

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(
                    u => u.UsuarioRed.ToLower() == trimmed || u.Correo.ToLower() == trimmed,
                    cancellationToken);

            if (user is null || !user.Activo)
            {
                return PasswordResetRequestResult.NotFound();
            }

            var previousTokens = await _context.PasswordResetTokens
                .Where(t => t.UsuarioId == user.Id && t.FechaUso == null)
                .ToListAsync(cancellationToken);

            foreach (var oldToken in previousTokens)
            {
                oldToken.FechaUso = DateTime.UtcNow;
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var tokenHash = Sha256IntegrityService.ComputeHash(token).ToUpperInvariant();
            var expiration = DateTime.UtcNow.AddMinutes(30);

            var resetToken = new PasswordResetToken
            {
                UsuarioId = user.Id,
                TokenHash = tokenHash,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = expiration,
                FechaUso = null
            };

            await _context.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return PasswordResetRequestResult.CreatedSuccessfully(user.Correo, token, expiration);
        }

        public async Task<PasswordResetResult> ResetPasswordAsync(
            string token,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            {
                return PasswordResetResult.Invalid();
            }

            var tokenHash = Sha256IntegrityService.ComputeHash(token).ToUpperInvariant();

            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

            if (resetToken is null || resetToken.FechaUso.HasValue)
            {
                return PasswordResetResult.Invalid();
            }

            if (resetToken.FechaExpiracion <= DateTime.UtcNow)
            {
                return PasswordResetResult.ExpiredToken();
            }

            if (resetToken.Usuario is null || !resetToken.Usuario.Activo)
            {
                return PasswordResetResult.Invalid();
            }

            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            resetToken.Usuario.PasswordHash = newPasswordHash;
            resetToken.FechaUso = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return PasswordResetResult.Completed();
        }
    }

    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(
            string toEmail,
            string resetLink,
            CancellationToken cancellationToken = default);
    }

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
}
