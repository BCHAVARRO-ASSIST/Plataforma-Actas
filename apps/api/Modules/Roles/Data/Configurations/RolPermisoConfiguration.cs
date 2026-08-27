using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Modules.Roles.Data.Configurations;

public sealed class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("rol_permisos");

        builder.HasKey(rp => new { rp.RolId, rp.PermisoId })
            .HasName("PK_rol_permisos");

        builder.Property(rp => rp.RolId)
            .HasColumnName("RolId")
            .IsRequired();

        builder.Property(rp => rp.PermisoId)
            .HasColumnName("PermisoId")
            .IsRequired();
    }
}
