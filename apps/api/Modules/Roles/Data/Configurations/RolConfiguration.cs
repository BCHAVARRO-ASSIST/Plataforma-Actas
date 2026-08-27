using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Modules.Roles.Data.Configurations;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id)
            .HasName("PK_roles");

        builder.Property(r => r.Id)
            .HasColumnName("Id")
            .UseIdentityByDefaultColumn();

        builder.Property(r => r.Nombre)
            .HasColumnName("Nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_roles_nombre");

        builder.Property(r => r.Descripcion)
            .HasColumnName("Descripcion")
            .HasMaxLength(250);

        builder.Property(r => r.Activo)
            .HasColumnName("Activo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(r => r.FechaCreacion)
            .HasColumnName("FechaCreacion")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasMany(r => r.RolPermisos)
            .WithOne(rp => rp.Rol)
            .HasForeignKey(rp => rp.RolId)
            .HasConstraintName("FK_rol_permisos_rol")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.UsuarioRoles)
            .WithOne(ur => ur.Rol)
            .HasForeignKey(ur => ur.RolId)
            .HasConstraintName("FK_usuario_roles_rol")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
