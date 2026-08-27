using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PlataformaActas.Api.Infrastructure.Persistence;
using PlataformaActas.Api.Infrastructure.Security;
using PlataformaActas.Api.Modules.Auth.Data.Entities;
using PlataformaActas.Api.Modules.Auth.Models.Dto;

namespace PlataformaActas.Api.Modules.Auth.Services;

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

        // Invalidate prior unused tokens
        var previousTokens = await _context.PasswordResetTokens
            .Where(t => t.UsuarioId == user.Id && t.FechaUso == null)
            .ToListAsync(cancellationToken);

        foreach (var oldToken in previousTokens)
        {
            oldToken.FechaUso = DateTime.UtcNow;
        }

        // Generate cryptographically strong token
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
