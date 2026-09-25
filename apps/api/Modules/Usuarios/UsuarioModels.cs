using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlataformaActas.Api.Modules.Usuarios.Data.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string UsuarioRed { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? NumeroDocumento { get; set; }
        public string? PasswordHash { get; set; }
        public string Proceso { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<PlataformaActas.Api.Modules.Roles.Data.Entities.UsuarioRol> UsuarioRoles { get; set; } = new List<PlataformaActas.Api.Modules.Roles.Data.Entities.UsuarioRol>();
        public ICollection<PlataformaActas.Api.Modules.Auth.Data.Entities.PasswordResetToken> PasswordResetTokens { get; set; } = new List<PlataformaActas.Api.Modules.Auth.Data.Entities.PasswordResetToken>();
    }
}

namespace PlataformaActas.Api.Modules.Usuarios.Data.Configurations
{
    using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("usuarios");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Apellido)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.UsuarioRed)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(u => u.UsuarioRed)
                .IsUnique();

            builder.Property(u => u.Correo)
                .HasMaxLength(150)
                .IsRequired();

            builder.HasIndex(u => u.Correo)
                .IsUnique();

            builder.Property(u => u.NumeroDocumento)
                .HasColumnName("numero_documento")
                .HasMaxLength(30);

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(255);

            builder.Property(u => u.Proceso)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}

namespace PlataformaActas.Api.Modules.Usuarios.Models.Dto
{
    public record UsuarioCreateRequest(string Nombre, string Apellido, string UsuarioRed, string Correo, string Proceso, string? Password = null, int? RolId = null, string? NumeroDocumento = null);
    public record UsuarioUpdateRequest(string Nombre, string Apellido, string UsuarioRed, string Correo, string Proceso, bool Activo, string? NumeroDocumento = null);
    public record UsuarioResetPasswordRequest(string NewPassword, string ConfirmPassword);
    public record UsuarioRolRequest(int RolId);
    public record UsuarioResponse(int Id, string Nombre, string Apellido, string UsuarioRed, string Correo, string Proceso, bool Activo, DateTime FechaCreacion, string? RolNombre, int? RolId, string? NumeroDocumento = null);
}

namespace PlataformaActas.Api.Modules.Usuarios.Models.Mappers
{
    using PlataformaActas.Api.Modules.Usuarios.Data.Entities;
    using PlataformaActas.Api.Modules.Usuarios.Models.Dto;

    public static class UsuarioMappingExtensions
    {
        public static UsuarioResponse ToResponse(this Usuario usuario)
        {
            var rolAsignado = usuario.UsuarioRoles.FirstOrDefault();

            return new UsuarioResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Apellido,
                usuario.UsuarioRed,
                usuario.Correo,
                usuario.Proceso,
                usuario.Activo,
                usuario.FechaCreacion,
                rolAsignado?.Rol?.Nombre,
                rolAsignado?.RolId,
                usuario.NumeroDocumento
            );
        }
    }
}
