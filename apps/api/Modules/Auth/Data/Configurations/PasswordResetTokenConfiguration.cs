using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Auth.Data.Entities;

namespace PlataformaActas.Api.Modules.Auth.Data.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(t => t.Id)
            .HasName("PK_password_reset_tokens");

        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .UseIdentityByDefaultColumn();

        builder.Property(t => t.UsuarioId)
            .HasColumnName("UsuarioId")
            .IsRequired();

        builder.HasIndex(t => t.UsuarioId)
            .HasDatabaseName("IX_password_reset_tokens_UsuarioId");

        builder.Property(t => t.TokenHash)
            .HasColumnName("TokenHash")
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash)
            .HasDatabaseName("IX_password_reset_tokens_TokenHash");

        builder.Property(t => t.FechaCreacion)
            .HasColumnName("FechaCreacion")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(t => t.FechaExpiracion)
            .HasColumnName("FechaExpiracion")
            .IsRequired();

        builder.HasIndex(t => t.FechaExpiracion)
            .HasDatabaseName("IX_password_reset_tokens_FechaExpiracion");

        builder.Property(t => t.FechaUso)
            .HasColumnName("FechaUso");

        builder.HasOne(t => t.Usuario)
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .HasConstraintName("FK_password_reset_tokens_usuarios")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
