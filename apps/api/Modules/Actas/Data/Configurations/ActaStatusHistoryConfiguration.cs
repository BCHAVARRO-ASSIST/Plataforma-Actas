using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaActas.Api.Modules.Actas.Data.Entities;

namespace PlataformaActas.Api.Modules.Actas.Data.Configurations;

public sealed class ActaStatusHistoryConfiguration : IEntityTypeConfiguration<ActaStatusHistory>
{
    public void Configure(EntityTypeBuilder<ActaStatusHistory> builder)
    {
        builder.ToTable("acta_status_history_v2");

        builder.HasKey(h => h.Id)
            .HasName("acta_status_history_v2_pkey");

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(h => h.ActaId)
            .HasColumnName("acta_id")
            .IsRequired();

        builder.HasIndex(h => h.ActaId)
            .HasDatabaseName("ix_acta_status_history_v2_acta_id");

        builder.Property(h => h.EstadoAnterior)
            .HasColumnName("estado_anterior")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.EstadoNuevo)
            .HasColumnName("estado_nuevo")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(h => h.UsuarioId)
            .HasColumnName("usuario_id");

        builder.HasIndex(h => h.UsuarioId)
            .HasDatabaseName("ix_acta_status_history_v2_usuario_id");

        builder.Property(h => h.Comentario)
            .HasColumnName("comentario")
            .HasMaxLength(1000);

        builder.Property(h => h.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .HasConstraintName("fk_acta_status_history_v2_usuario")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
