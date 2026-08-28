using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlataformaActas.Api.Modules.Auth.Data.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public PlataformaActas.Api.Modules.Usuarios.Data.Entities.Usuario Usuario { get; set; } = null!;

        public string TokenHash { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaUso { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}

namespace PlataformaActas.Api.Modules.Auth.Data.Configurations
{
    using PlataformaActas.Api.Modules.Auth.Data.Entities;

    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("password_reset_tokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TokenHash)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasIndex(t => t.TokenHash);

            builder.Property(t => t.FechaExpiracion)
                .IsRequired();

            builder.Property(t => t.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.HasOne(t => t.Usuario)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

namespace PlataformaActas.Api.Modules.Auth.Models.Dto
{
    public sealed record LoginRequest(
        string Usuario,
        string Password
    );

    public sealed record LoginResponse(
        string Token,
        UsuarioAuthResponse Usuario,
        RolAuthResponse? Rol,
        IReadOnlyList<string> Permisos
    );

    public sealed record UsuarioAuthResponse(
        int Id,
        string Nombre,
        string Apellido,
        string UsuarioRed,
        string Correo,
        string Proceso
    );

    public sealed record RolAuthResponse(
        int Id,
        string Nombre,
        string? Descripcion
    );

    public sealed record ForgotPasswordRequest(
        string Identifier
    );

    public sealed record ResetPasswordRequest(
        string Token,
        string NewPassword,
        string ConfirmPassword
    );

    public sealed record LoginData(
        UsuarioAuthResponse Usuario,
        RolAuthResponse? Rol,
        IReadOnlyList<string> Permisos
    );

    public sealed record PasswordResetRequestResult(
        bool Created,
        bool UserFound,
        string? Correo,
        string? Token,
        DateTime? Expiration)
    {
        public static PasswordResetRequestResult NotFound() =>
            new(false, false, null, null, null);

        public static PasswordResetRequestResult CreatedSuccessfully(
            string correo,
            string token,
            DateTime expiration) =>
            new(true, true, correo, token, expiration);
    }

    public sealed record PasswordResetResult(
        bool Successful,
        bool IsExpired)
    {
        public static PasswordResetResult Completed() =>
            new(true, false);

        public static PasswordResetResult Invalid() =>
            new(false, false);

        public static PasswordResetResult Expired() =>
            new(false, true);

        public static PasswordResetResult ExpiredToken() =>
            new(false, true);
    }

    public record MessageResponse(string Message);
    public record AuthenticatedUserResponse(int Id, string Nombre, string Apellido, string UsuarioRed, string Correo, string Proceso, string? Rol, IReadOnlyList<string> Permisos);
}
