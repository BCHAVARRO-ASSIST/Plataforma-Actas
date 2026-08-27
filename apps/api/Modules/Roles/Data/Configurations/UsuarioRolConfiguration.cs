using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Roles.Data.Entities;

namespace PlataformaActas.Api.Modules.Roles.Data.Configurations;

public sealed class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("usuario_roles");

        builder.HasKey(ur => new { ur.UsuarioId, ur.RolId })
            .HasName("PK_usuario_roles");

        builder.Property(ur => ur.UsuarioId)
            .HasColumnName("UsuarioId")
            .IsRequired();

        builder.Property(ur => ur.RolId)
            .HasColumnName("RolId")
            .IsRequired();

        builder.Property(ur => ur.FechaAsignacion)
            .HasColumnName("FechaAsignacion")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}
