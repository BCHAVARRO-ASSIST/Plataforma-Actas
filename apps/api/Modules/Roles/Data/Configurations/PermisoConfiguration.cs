using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Modules.Roles.Data.Configurations;

public sealed class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("permisos");

        builder.HasKey(p => p.Id)
            .HasName("PK_permisos");

        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .UseIdentityByDefaultColumn();

        builder.Property(p => p.Nombre)
            .HasColumnName("Nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(p => p.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_permisos_nombre");

        builder.Property(p => p.Modulo)
            .HasColumnName("Modulo")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Descripcion)
            .HasColumnName("Descripcion")
            .HasMaxLength(250);

        builder.Property(p => p.Activo)
            .HasColumnName("Activo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasMany(p => p.RolPermisos)
            .WithOne(rp => rp.Permiso)
            .HasForeignKey(rp => rp.PermisoId)
            .HasConstraintName("FK_rol_permisos_permiso")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
