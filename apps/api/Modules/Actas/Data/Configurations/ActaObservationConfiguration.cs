using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Actas.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data.Configurations;

public sealed class ActaObservationConfiguration : IEntityTypeConfiguration<ActaObservation>
{
    public void Configure(EntityTypeBuilder<ActaObservation> builder)
    {
        builder.ToTable("acta_observations_v2");

        builder.HasKey(o => o.Id)
            .HasName("acta_observations_v2_pkey");

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(o => o.ActaId)
            .HasColumnName("acta_id")
            .IsRequired();

        builder.HasIndex(o => o.ActaId)
            .HasDatabaseName("ix_acta_observations_v2_acta_id");

        builder.Property(o => o.Texto)
            .HasColumnName("texto")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(o => o.UsuarioId)
            .HasColumnName("usuario_id");

        builder.HasIndex(o => o.UsuarioId)
            .HasDatabaseName("ix_acta_observations_v2_usuario_id");

        builder.Property(o => o.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasOne(o => o.Usuario)
            .WithMany()
            .HasForeignKey(o => o.UsuarioId)
            .HasConstraintName("fk_acta_observations_v2_usuario")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
