using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Usuarios.Data.Entities;

namespace PlataformaActas.Api.Modules.Usuarios.Data.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id)
            .HasName("PK_usuarios");

        builder.Property(u => u.Id)
            .HasColumnName("Id")
            .UseIdentityByDefaultColumn();

        builder.Property(u => u.Nombre)
            .HasColumnName("Nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Apellido)
            .HasColumnName("Apellido")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.UsuarioRed)
            .HasColumnName("UsuarioRed")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(u => u.UsuarioRed)
            .IsUnique()
            .HasDatabaseName("IX_usuarios_UsuarioRed");

        builder.Property(u => u.Correo)
            .HasColumnName("Correo")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(u => u.Correo)
            .IsUnique()
            .HasDatabaseName("IX_usuarios_Correo");

        builder.Property(u => u.Proceso)
            .HasColumnName("Proceso")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Activo)
            .HasColumnName("Activo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasMaxLength(255);

        builder.Property(u => u.UltimoAcceso)
            .HasColumnName("UltimoAcceso");

        builder.Property(u => u.FechaCreacion)
            .HasColumnName("FechaCreacion")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasMany(u => u.UsuarioRoles)
            .WithOne(ur => ur.Usuario)
            .HasForeignKey(ur => ur.UsuarioId)
            .HasConstraintName("FK_usuario_roles_usuario")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
